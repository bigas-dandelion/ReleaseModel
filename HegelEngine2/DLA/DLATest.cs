using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;
using HegelEngine2.ReleaseModel;
using HegelEngine2.Utils;
using System.Text;

namespace HegelEngine2.DLA;
public class DLATest : CellularAutomaton
{
    private readonly int _porosity;
    private readonly float _reactProb;
    private readonly string _fileName;
    private readonly int _boot;

    private int _totalPorosity;

    private Random _random = new Random();

    private int _x = 0, _y = 0, _z = 0;

    private int _countRedCells = 0;

    private int _solidCells;

    private readonly int _size;

    private List<VectorInt> clusterPositions = new List<VectorInt>();

    private List<VectorInt> takenPositions = new List<VectorInt>();

    private DLAViewModel _inputParams;

    private string _directoryPath = "tests";

    public DLATest(ModelView output, ViewModel input) : base(output, input)
    {
        _inputParams = (DLAViewModel)input;

        _porosity = _inputParams.Porosity;

        _statesNumbers.Add("solution", 0);
        _statesNumbers.Add("nonsoluble", -1);
        _statesNumbers.Add("movable", -2);

        _size = _inputParams.Size.X;
        _reactProb = _inputParams.ReactionProbability;
        _fileName = _inputParams.FileName;
        _boot = _inputParams.BootParam;
    }

    public override void CreateInitialConfiguration()
    {
        var size = _inputParams.Size.X;
        var c = new VectorInt(size / 2, size / 2, 0);
        var weight = _inputParams.SolidMass;

        clusterPositions.Add(c);

        ProcessField((x, y, z) =>
        {
            _outputParameters.FieldAG[x, y, z].State = (x == c.X && y == c.Y && z == c.Z)
                ? StatesNumbers["nonsoluble"] : StatesNumbers["solution"];
        });

        CreateMovableCell();
    }

    private void CreateMovableCell()
    {
        do
        {
            _x = _random.Next(_size);
            _y = _random.Next(_size);
        } while (clusterPositions.Contains(new VectorInt(_x, _y, _z)));

        _outputParameters.FieldAG[_x, _y, _z].State = StatesNumbers["movable"];
    }

    private void TransformRandomSolutionNeighborToCluster()
    {
        foreach (var n in _neighbors)
        {
            VectorInt checkPos = new VectorInt(_x + n.X, _y + n.Y, _z + n.Z);

            if (checkPos.X >= 0 && checkPos.X < _size &&
                checkPos.Y >= 0 && checkPos.Y < _size)
            {
                ProcessBorder(checkPos.X, checkPos.Y, checkPos.Z, out float nextState);

                if (nextState == StatesNumbers["solution"])
                {
                    takenPositions.Add(checkPos);
                }
            }
        }

        if (takenPositions.Count > 0)
        {
            var el = takenPositions[_random.Next(takenPositions.Count)];

            GetCell(el).State = _inputParams.SolidMass;

            _solidCells++;

            takenPositions.Clear();
        }

        (_outputParameters as DLAModelView).SolidCells = _solidCells;
    }

    public void WriteFieldToFile(string fileName)
    {
        if (!Directory.Exists(_directoryPath))
        {
            Directory.CreateDirectory(_directoryPath);
        }

        string fullPath = Path.Combine(_directoryPath, fileName);

        using (StreamWriter writer = new StreamWriter(fullPath, false, Encoding.UTF8))
        {
            for (int z = 0; z < _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Z); z++)
            {
                for (int y = 0; y < _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Y); y++)
                {
                    for (int x = 0; x < _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.X); x++)
                    {
                        float state = _outputParameters.FieldAG[x, y, z].State;
                        writer.WriteLine(string.Join("\t", new string[] {x.ToString(),y.ToString(),
                                z.ToString(), state.ToString()}));
                    }
                }
            }

            writer.WriteLine(_solidCells.ToString());
        }

        Console.WriteLine($"Файл успешно сохранён по пути: {fullPath}");
    }

    private void Cut()
    {
        _solidCells = 0;

        var size = _inputParams.Size.X;
        var center = new VectorInt(size / 2, size / 2, 0);
        var radius = _inputParams.Diameter / 2;
        var weight = _inputParams.SolidMass;

        ProcessField((x, y, z) => 
        {
            var distanceSq = ((x - center.X) * (x - center.X)) +
                 ((y - center.Y) * (y - center.Y));

            if (!(distanceSq <= radius * radius))
            {
                _outputParameters.FieldAG[x, y, z].State = StatesNumbers["solution"];
            }


            if (_outputParameters.FieldAG[x, y, z].State == weight)
            {
                _solidCells++;
            }
        });

        (_outputParameters as DLAModelView).SolidCells = _solidCells;
    }

    public override void Update()
    {
        if (IsFinished())
        {
            (_outputParameters as DLAModelView).IsEnd = true;

            Cut();

            WriteFieldToFile(_fileName);

            return;
        }

        var checkPos = new VectorInt(0, 0, 0);
        foreach (var n in _neighbors)
        {
            checkPos = new VectorInt(_x + n.X, _y + n.Y, _z + n.Z);

            if (checkPos.X >= 0 && checkPos.X < _size &&
                checkPos.Y >= 0 && checkPos.Y < _size)
            {
                ProcessBorder(checkPos.X, checkPos.Y, checkPos.Z, out float nextState);

                if (nextState == StatesNumbers["solution"])
                {
                    takenPositions.Add(checkPos);
                }
                else if (nextState == StatesNumbers["nonsoluble"])
                {
                    takenPositions.Clear();
                    _totalPorosity++;
                    GetCell(_x, _y, _z).State = StatesNumbers["nonsoluble"];
                    clusterPositions.Add(new VectorInt(_x, _y, _z));

                    if ((float)_random.NextDouble() >= _reactProb)
                    {
                        TransformRandomSolutionNeighborToCluster();
                        _countRedCells++;
                    }

                    if (_totalPorosity < _porosity)
                    {
                        CreateMovableCell();
                    }

                    return;
                }
            }
        }

        if (takenPositions.Count > 0)
        {
            var el = takenPositions[_random.Next(takenPositions.Count)];

            var neighState = GetCell(el.X, el.Y, el.Z).State;

            GetCell(el.X, el.Y, el.Z).State = StatesNumbers["movable"];
            GetCell(_x, _y, _z).State = StatesNumbers["solution"];
            (_x, _y, _z) = (el.X, el.Y, el.Z);
        }

        takenPositions.Clear();
    }

    public override bool IsFinished()
    {
        return _porosity == _totalPorosity && _boot == _countRedCells;
    }
}