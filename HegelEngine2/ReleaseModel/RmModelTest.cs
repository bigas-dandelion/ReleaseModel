using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;
using HegelEngine2.Utils;
using System.Text;

namespace HegelEngine2.ReleaseModel;

public class RmModelTest : CellularAutomaton
{
    private float[,,] _bufferField;

    private float _solidCells = 0;
    private float _releasedMass = 0;
    private readonly float _liquidMass;
    private readonly float _k;
    private readonly float _d;

    private RmViewModel _inputParams;

    public RmModelTest(ModelView output, ViewModel input) : base(output, input)
    {
        _inputParams = (RmViewModel)input;

        _statesNumbers.Add("solution", 0);

        _liquidMass = (_inputParameters as RmViewModel).LiquidMass;
        _k = (_inputParameters as RmViewModel).K;
        _d = (_inputParameters as RmViewModel).D;
    }

    public override void CreateInitialConfiguration()
    {
        _releasedMass = 0;
        var size = _inputParams.Size.X;
        var center = new VectorInt(size / 2, size / 2, 0);
        var radius = _inputParams.Diameter / 2;
        var weight = _inputParams.SolidMass;

        ProcessField((x, y, z) =>
        {
            var distanceSq = ((x - center.X) * (x - center.X)) +
                             ((y - center.Y) * (y - center.Y));
            _outputParameters.FieldAG[x, y, z].State =
                distanceSq <= radius * radius ? weight : StatesNumbers["solution"];
        });
    }

    private List<(VectorInt pos, float potentialDiff)> searchingCandidates
                    = new List<(VectorInt pos, float potentialDiff)>();

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
                currentMass > nextMass)
            {
                float diff = _d * (tmpMass - nextMass);

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

    public override void Update()
    {
        if (_outputParameters.IsFinished)
        {
            (_outputParameters as RmModelView).IsEnd = true;
            return;
        }

        _bufferField = new float[_inputParameters.Size.X,
                _inputParameters.Size.Y, _inputParameters.Size.Z];

        _solidCells = 0;

        ProcessField(ChoiceMethod);

        ProcessField((x, y, z) =>
        {
            GetCell(x, y, z).State += _bufferField[x, y, z];
        });

        (_outputParameters as RmModelView)._iterMass[_outputParameters.Iteration] = _releasedMass;

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