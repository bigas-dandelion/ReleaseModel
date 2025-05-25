using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;
using HegelEngine2.Utils;

namespace HegelEngine2.ReleaseModel;

public class RmModelTest : CellularAutomaton
{
    private float[,,] _bufferField;

    private float _solidCells = 0;
    private float _releasedMass = 0;
    private float _liquidMass;
    private readonly int _size;

    private readonly float _k;
    private float _D;
    private readonly float _cSatur;
    private readonly float _dt;
    private readonly float _dx;
    private readonly bool _porosity;
    private Action _initialConfigurationAction { get; }

    private float M_max;

    private RmViewModel _inputParams;

    private List<(VectorInt pos, float potentialDiff)> searchingCandidates
                = new List<(VectorInt pos, float potentialDiff)>();

    private string directoryPath = "tests";

    public RmModelTest(ModelView output, ViewModel input) : base(output, input)
    {
        _inputParams = (RmViewModel)input;

        _statesNumbers.Add("solution", 0);
        _statesNumbers.Add("nonsoluble", -1);
        _statesNumbers.Add("movable", -2);

        _size = _inputParams.Size.X;

        if (_inputParams.IsTherePorosity)
        {
            _initialConfigurationAction = () => Calc();
            _inputParameters.InputField = LoadFile(_inputParams.FileName);
        }
        else
        {
            InitInputField();
            _initialConfigurationAction = () => CreateTablet();
        }

        _cSatur = _inputParams.SaturatedConc;
        _D = _inputParams.D;
        _k = _inputParams.K;
        _dt = _inputParams.dt;
        _dx = _inputParams.dx;
    }

    private void InitInputField()
    {
        _inputParameters.InputField = new float[_inputParams.Size.X, 
                                    _inputParams.Size.Y, _inputParams.Size.Z];

        ProcessInputField((x, y, z) => 
        {
            _inputParameters.InputField[x, y, z] = StatesNumbers["solution"];
        });
    }

    private void Calc()
    {
        M_max = _cSatur * _dx * _dx * _dx;
        _D = _D * _dt / (_dx * _dx);

        _liquidMass = (_inputParameters as RmViewModel).LiquidMass = M_max;

        _releasedMass = 0;
    }

    private void CreateTablet()
    {
        Calc();

        var size = _inputParams.Size.X;
        var center = new VectorInt(size / 2, size / 2, 0);
        var radius = _inputParams.Diameter / 2;
        var weight = _inputParams.SolidMass;

        ProcessInputField((x, y, z) => 
        {
            var distanceSq = ((x - center.X) * (x - center.X)) +
                            ((y - center.Y) * (y - center.Y));

            var state = distanceSq <= radius * radius ? weight : StatesNumbers["solution"];
            _inputParameters.InputField[x, y, z] = state;

            if (state == weight)
            {
                _solidCells++;
            }
        });

        (_outputParameters as RmModelView).SolidCells = _solidCells;
    }

    private float[,,] LoadFile(string fileName)
    {
        string fullPath = Path.Combine(directoryPath, fileName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Файл не найден: {fullPath}");

        var lines = File.ReadAllLines(fullPath);

        float[,,] field = new float[_inputParams.Size.X, _inputParams.Size.Y, _inputParams.Size.Z];

        for (int i = 0; i < lines.Length - 1; i++)
        {
            var parts = lines[i].Split('\t');
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            int z = int.Parse(parts[2]);
            float state = float.Parse(parts[3]);

            field[x, y, z] = state;
        }

        _solidCells = int.Parse(lines[^1]);

        (_outputParameters as RmModelView).SolidCells = _solidCells;

        return field;
    }

    public override void CreateInitialConfiguration()
    {
        _initialConfigurationAction?.Invoke();
    }

    public void Diffuse(int x, int y, int z)
    {
        float currentMass = GetCell(x, y, z).State;
        if (currentMass == 0)
            return;
        float tmpMass = currentMass;

        float totalDiffusion = 0f;

        searchingCandidates.Clear();

        foreach (var n in _neighbors)
        {
            var checkPos = new VectorInt(x + n.X, y + n.Y, z + n.Z);
            ProcessBorder(checkPos.X, checkPos.Y, checkPos.Z, out float nextMass);

            if (nextMass != StatesNumbers["border"] &&
                nextMass <= _liquidMass &&
                currentMass > nextMass && nextMass != StatesNumbers["nonsoluble"])
            {
                float diff = _D * (tmpMass - nextMass);

                tmpMass -= diff;

                searchingCandidates.Add((checkPos, diff));
                totalDiffusion += diff;
            }
        }
        if (searchingCandidates.Count == 0)
            return;

        if (tmpMass <= 0)
        {
            float distributedMass = 0f;

            foreach (var (pos, diff) in searchingCandidates)
            {
                float proportion = diff / totalDiffusion;
                float actualTransfer = currentMass * proportion;
                currentMass -= actualTransfer;

                _bufferField[pos.X, pos.Y, pos.Z] += actualTransfer;
                distributedMass += actualTransfer;
            }

            _bufferField[x, y, z] = (_bufferField[x, y, z] - distributedMass < 0)
                ? 0 : _bufferField[x, y, z] - distributedMass;
        }
        else
        {
            foreach (var (pos, diff) in searchingCandidates)
            {
                _bufferField[x, y, z] -= diff;
                _bufferField[pos.X, pos.Y, pos.Z] += diff;
            }
        }
    }

    public void Dilute(int x, int y, int z)
    {
        _solidCells++;

        var curState = _inputParameters.InputField[x, y, z];

        var checkPos = new VectorInt(0, 0, 0);
        foreach (var n in _neighbors)
        {
            checkPos = new VectorInt(x + n.X, y + n.Y, z + n.Z);

            if (checkPos.X >= 0 && checkPos.X < _size &&
                    checkPos.Y >= 0 && checkPos.Y < _size)
            {
                ProcessBorder(checkPos.X, checkPos.Y, checkPos.Z, out float nextState);

                if (nextState < _liquidMass && nextState != StatesNumbers["nonsoluble"])
                {
                    float diffMass = _k * (_liquidMass - nextState);
                    _bufferField[x, y, z] -= diffMass;
                    _bufferField[checkPos.X, checkPos.Y, checkPos.Z] += diffMass;
                }
            }
        }
    }

    public void ChoiceMethod(int x, int y, int z)
    {
        float state = _inputParameters.InputField[x, y, z];

        if (state == StatesNumbers["nonsoluble"])
        {
            return;
        }
        else if (state > _liquidMass)
        {
            Dilute(x, y, z);
        }
        else
        {
            _releasedMass += state;
            Diffuse(x, y, z);
        }
    }

    private void ProcessInputField(ProcessCell applyFunction)
    {
        for (int z = 0; z < _inputParameters.InputField.GetLength((int)VectorInt.Dimension.Z); z++)
        {
            for (int y = 0; y < _inputParameters.InputField.GetLength((int)VectorInt.Dimension.Y); y++)
            {
                for (int x = 0; x < _inputParameters.InputField.GetLength((int)VectorInt.Dimension.X); x++)
                {
                    applyFunction(x, y, z);
                }
            }
        }
    }

    public override void Update()
    {
        if (IsFinished())
        {
            (_outputParameters as RmModelView).IsEnd = true;
            return;
        }

        _bufferField = new float[_inputParameters.Size.X,
                _inputParameters.Size.Y, _inputParameters.Size.Z];

        _solidCells = 0;

        ProcessInputField(ChoiceMethod);

        ProcessInputField((x, y, z) =>
        {
            _inputParameters.InputField[x, y, z] += _bufferField[x, y, z];
        });

        (_outputParameters as RmModelView)._iterMass[_outputParameters.Iteration] = _releasedMass;

        (_outputParameters as RmModelView).SolidCells = _solidCells;

        _releasedMass = 0;

        base.Update();
    }

    public override bool IsFinished()
    {
        return (_outputParameters as RmModelView).SolidCells == 0;
    }
}