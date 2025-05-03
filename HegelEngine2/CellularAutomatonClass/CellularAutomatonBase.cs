using HegelEngine2.ParametersClasses;
using HegelEngine2.Utils;

namespace HegelEngine2.CellularAutomatonClass;

public abstract partial class CellularAutomaton
{
    protected delegate void ProcessCell(int x, int y, int z);

    protected ModelView _outputParameters;
    protected ViewModel _inputParameters;

    protected Dictionary<string, float> _statesNumbers = new Dictionary<string, float>()
        {
            { "border", float.NegativeInfinity },
            { "empty", 0 },
        };
    public IReadOnlyDictionary<string, float> StatesNumbers => _statesNumbers;
    protected List<VectorInt> _neighbors;

    public bool IsInitialized { get; private set; }

    public Action? Initialized;
    public Action? Updated;
    public Action? Finished;
    public CellularAutomaton (ModelView output, ViewModel input)
    {
        _outputParameters = output;
        _inputParameters = input;
        _outputParameters.State = AutomatonState.Empty;
    }
    public virtual void Update()
    {
        _outputParameters.Iteration++;
        if (IsFinished())
        {
            _outputParameters.State = AutomatonState.Finished;
        }        

        if (_outputParameters.IsFinished)
        {
            _outputParameters.State = AutomatonState.Finished;
            Finished?.Invoke();
        }
        else
        {
            _outputParameters.State = AutomatonState.Processing;
            Updated?.Invoke();
        }
    }
    public abstract bool IsFinished();

    /// <summary>
    /// Проверяет, является ли поле автомата трехмерным
    /// </summary>
    /// <returns>Возвращает <c>true</c>, если поле трехмерное</returns>
    public bool Is3D()
    {
        return _inputParameters.Size.Z > 1;
    }
    public int CalculateVolume()
    {
        return _inputParameters.Size.X * _inputParameters.Size.Y * _inputParameters.Size.Z;
    }
    #region Инициализация автомата
    /// <summary>
    /// Создает пустое поле с клетками с состоянием Empty, используя размер из <see cref="_inputParameters"/>
    /// </summary>
    public virtual void CreateEmptyField()
    {
        _outputParameters.FieldAG = new Cell[_inputParameters.Size.X, _inputParameters.Size.Y, _inputParameters.Size.Z];
        ProcessField(
            (x, y, z) =>
            _outputParameters.FieldAG[x, y, z] = new Cell(new VectorInt(x, y, z), StatesNumbers["empty"])
        );
    }
    /// <summary>
    /// Задает клеткам начальное состояние
    /// </summary>
    public abstract void CreateInitialConfiguration();
    /// <summary>
    /// Задает клеткам начальное состояние с использованием поля извне
    /// </summary>
    /// <param name="field">Презаданное поле</param>
    public virtual void CreateInitialConfiguration(float[,,] field)
    {
        CreateInitialConfiguration();
        ProcessField((x, y, z) => GetCell(x, y, z).State = field[x, y, z]);
    }
    /// <summary>
    /// Основной метод для инициализации автомата
    /// </summary>
    public void InitializeAutomata()
    {
        CreateEmptyField();        
        InitializeNeighborhood();
        InitializeParameters();
        if (_inputParameters.InputField == null)
            CreateInitialConfiguration();
        else
            CreateInitialConfiguration(_inputParameters.InputField);
        Initialized?.Invoke();
    }
    
    /// <summary>
    /// Инвоучит событие завершения инициализации
    /// </summary>
    public void OnInitialized ()
    {
        Initialized?.Invoke();
    }
    /// <summary>
    /// Инициализирует список относительных координат клеток окрестности <see cref="_neighbors"/> в соответствии с типом окрестности из <see cref="_inputParameters"/>
    /// </summary>
    protected virtual void InitializeNeighborhood()
    {
        switch (_inputParameters.Neighborhood)
        {
            case NeighborType.Neiman:
                {
                    _neighbors = NeimanGenerate(_inputParameters.NeighborhoodOrder);
                    break;
                }
            case NeighborType.Moore:
            default:
                {
                    _neighbors = MooreGenerate(_inputParameters.NeighborhoodOrder);
                    break;
                }
        }

        if (Is3D() == false)
            RemoveOZCoords(_neighbors);
    }
    /// <summary>
    /// Задает параметрам автомата начальные значения
    /// </summary>
    protected virtual void InitializeParameters ()
    {
        _outputParameters.Iteration = 0;
        _outputParameters.State = AutomatonState.Ready;        
    }
    #endregion
    #region Работа с клетками
    public void ChangeState(string state, VectorInt pos)
    {
        if (StatesNumbers.ContainsKey(state))
            GetCell(pos).State = StatesNumbers[state];
    }
    public Cell GetCell(VectorInt pos)
    {
        if (AreBordersCrossed(pos) == false)
            return _outputParameters.FieldAG[pos.X, pos.Y, pos.Z];

        return null;
    }
    public Cell GetCell(int x, int y, int z)
    {
        return GetCell(new VectorInt(x, y, z));       
    }

    protected void ProcessField(ProcessCell applyFunction)
    {
        for (int z = 0; z < _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Z); z++)
        {
            for (int y = 0; y < _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Y); y++)
            {
                for (int x = 0; x < _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.X); x++)
                {
                    applyFunction(x, y, z);
                }
            }
        }
    }
    #endregion  
    #region Работа с окрестностями
    public List<VectorInt> GetNeighbors()
    {
        return _neighbors;
    }

    /// <summary>
    /// Удаляет из списка клетки по оси OZ и делает соответствующее смещение
    /// </summary>
    /// <param name="neighborCells">Список координат, из которых нужно удалить OZ</param>    
    public static void RemoveOZCoords(List<VectorInt> neighborCells)
    {
        int centerZ = neighborCells.Max(cell => cell.Z) / 2;
        neighborCells.RemoveAll(cell => cell.Z != centerZ);
        neighborCells.ForEach(cell => cell.Z = 0);
    }

    /// <summary>
    /// Считает координаты клеток для окрестности Мура
    /// </summary>
    /// <param name="order">Порядок окрестности</param>
    /// <returns>Список относительных координат клеток окрестности</returns>
    public static List<VectorInt> MooreGenerate(byte order)
    {
        List<VectorInt> neighbors = new List<VectorInt>();
        for (int i = -order; i <= order; i++)
            for (int j = -order; j <= order; j++)
                for (int k = -order; k <= order; k++)
                {
                    if (i == 0 && j == 0 && k == 0) continue;//Сама клетка
                    neighbors.Add(new VectorInt(i, j, k));
                }
        return neighbors;
    }

    /// <summary>
    /// Считает координаты клеток для окрестности фон Неймана
    /// </summary>
    /// <param name="order">Порядок окрестности</param>
    /// <returns>Список относительных координат клеток окрестности</returns>
    public static List<VectorInt> NeimanGenerate(byte order)
    {
        List<VectorInt> neighbors = new List<VectorInt>();
        List<VectorInt> directions = new List<VectorInt>
        {
            new VectorInt(-1, 0, 0),
            new VectorInt(1, 0, 0) ,
            new VectorInt(0, 1, 0) ,
            new VectorInt(0, -1, 0),
            new VectorInt(0, 0, -1),
            new VectorInt(0, 0, 1)
        };
        byte wave = 1;
        byte[,,] neighborhoodField = new byte[2 * order + 1, 2 * order + 1, 2 * order + 1];
        neighborhoodField[order, order, order] = wave;
        while (wave <= order)
        {
            for (int i = 0; i < 2 * order; i++)
                for (int j = 0; j < 2 * order; j++)
                    for (int k = 0; k < 2 * order; k++)
                    {
                        if (neighborhoodField[i, j, k] == wave)
                        {
                            // Убирать ли саму клетку из окрестности
                            foreach (var d in directions)
                            {
                                neighborhoodField[i + d.X, j + d.Y, k + d.Z] = (byte)(wave + 1);
                                neighbors.Add(new VectorInt(i + d.X - order, j + d.Y - order, k + d.Z - order));
                            }
                        }
                    }
            wave++;
        }
        return neighbors;
    }
    public enum NeighborType : byte
    {
        Neiman,
        Moore
    }
    #endregion
    public enum AutomatonState : byte
    {
        Empty,
        Ready,
        Processing,
        Paused,
        Finished
    }
}
