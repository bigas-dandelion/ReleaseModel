using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;
using HegelEngine2.Utils;

namespace HegelEngine2.ReleaseModel;

public class RmModelTest : CellularAutomaton
{
    private float[,,] _bufferField;

    private float _solidCells = 0;
    private float _releasedMass = 0;
    private readonly float _liquidMass;
    private readonly float _k;
    private readonly float _d;

    private Dictionary<int, float> _iterMass = new Dictionary<int, float>();

    public RmModelTest(ModelView output, ViewModel input) : base(output, input)
    {
        _statesNumbers.Add("solution", 0);

        _liquidMass = (_inputParameters as RmViewModel).LiquidMass;
        _k = (_inputParameters as RmViewModel).K;
        _d = (_inputParameters as RmViewModel).D;
    }

    public override void CreateInitialConfiguration()
    {
        _releasedMass = 0;

        var d = (_inputParameters as RmViewModel).Diameter;

        var size = (_inputParameters as RmViewModel).Size.X;

        var weight = (_inputParameters as RmViewModel).SolidMass;

        float x0 = (float)Math.Floor((float)size / 2.0);
        float y0 = (float)Math.Floor((float)size / 2.0);
        float r = (float)Math.Floor(d / 2.0);

        ProcessField((x, y, z) =>
        {
            if ((Math.Pow(x - x0, 2) + Math.Pow(y - y0, 2)) <= r * r)
            {
                _outputParameters.FieldAG[x, y, z].State = weight;
            }
            else
            {
                _outputParameters.FieldAG[x, y, z].State = StatesNumbers["solution"];
            }
        });
    }

    private List<(VectorInt pos, float potentialDiff)> searchingCandidates 
                    = new List<(VectorInt pos, float potentialDiff)>();

    public void Diffuse(int x, int y, int z)
    {
        float currentMass = GetCell(x, y, z).State;

        float totalDiffusion = 0f;

        foreach (var n in _neighbors)
        {
            var checkPos = new VectorInt(x + n.X, y + n.Y, z + n.Z);
            ProcessBorder(checkPos.X, checkPos.Y, checkPos.Z, out float nextMass);

            if (nextMass != StatesNumbers["border"] &&
                nextMass <= _liquidMass &&
                currentMass > nextMass)
            {
                float diff = _d * (currentMass - nextMass);

                searchingCandidates.Add((checkPos, diff));
                totalDiffusion += diff;
            }
        }

        if (searchingCandidates.Count == 0)
            return;

        if ((currentMass - totalDiffusion) <= 0)
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

            _bufferField[x, y, z] -= distributedMass;
        }
        else
        {
            foreach (var (pos, diff) in searchingCandidates)
            {
                _bufferField[x, y, z] -= diff;
                _bufferField[pos.X, pos.Y, pos.Z] += diff;
            }
        }

        searchingCandidates.Clear();
    }

    public void Dilute(int x, int y, int z)
    {
        _solidCells++;

        var curState = GetCell(x, y, z).State;

        var checkPos = new VectorInt(0, 0, 0);
        foreach (var n in _neighbors)
        {
            checkPos = new VectorInt(x + n.X, y + n.Y, z + n.Z);

            ProcessBorder(checkPos.X, checkPos.Y, checkPos.Z, out float nextState);

            if (nextState < _liquidMass)
            {
                float diffMass = _k * (_liquidMass - nextState);
                _bufferField[x, y, z] -= diffMass;
                _bufferField[checkPos.X, checkPos.Y, checkPos.Z] += diffMass;
            }
        }
    }

    public void ChoiceMethod(int x, int y, int z)
    {
        float state = GetCell(x, y, z).State;

        if (state > _liquidMass)
        {
            Dilute(x, y, z);
        }
        else
        {
            _releasedMass += state;
            Diffuse(x, y, z);
        }
    }

    private float _leaved = 0;

    public override void Update()
    {
        if (_outputParameters.IsFinished)
        {
            using var writer = new StreamWriter("output.txt");
            writer.WriteLine("Индекс;Значение");
            foreach (var kvp in _iterMass.OrderBy(k => k.Key))
            {
                writer.WriteLine($"{kvp.Key}: {kvp.Value.ToString().Replace('.', ',')}");
            }
            return;
        }

        //if (_outputParameters.IsFinished)
        //{
        //    var lines = _iterMass.Select(kvp => $"{kvp.Key}: {kvp.Value}");
        //    File.WriteAllLines("output.txt", lines);
        //    return;
        //}

        _bufferField = new float[_inputParameters.Size.X, 
                _inputParameters.Size.Y, _inputParameters.Size.Z];

        _solidCells = 0;

        _releasedMass = _leaved;

        ProcessField(ChoiceMethod);

        ProcessField((x, y, z) => 
        {
            GetCell(x, y, z).State += _bufferField[x, y, z];
            if (x == 0 || y == 0 || x == _inputParameters.Size.X - 1 
                                    || y == _inputParameters.Size.Y - 1)
            {
                _leaved += GetCell(x, y, z).State;
                GetCell(x, y, z).State = 0;
            }
        });

        //_iterMass.Add(_outputParameters.Iteration, _releasedMass);
        _iterMass[_outputParameters.Iteration] = _releasedMass;

        (_outputParameters as RmModelView).SolidCells = _solidCells;

        _releasedMass = 0;

        base.Update();
    }

    public override bool IsFinished()
    {
        return _outputParameters.Iteration >= (_inputParameters as RmViewModel).Iterations ||
            (_outputParameters as RmModelView).SolidCells == 0;
    }
}