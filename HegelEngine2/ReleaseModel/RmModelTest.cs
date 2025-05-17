using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;
using HegelEngine2.Utils;
using System.Drawing;
using static IronPython.Modules._ast;

namespace HegelEngine2.ReleaseModel;

public class RmModelTest : CellularAutomaton
{
    private float[,,] _bufferField;

    private Random _random = new Random();

    private float _solidCells = 0;
    private float _releasedMass = 0;
    private float _liquidMass;
    private readonly int _size;

    private readonly float _k;
    private float _D;
    private readonly float _cSatur;
    private readonly float _dt;
    private readonly float _dx;
    private readonly int _porosity;
    private readonly float _reactProb;
    private Action _initialConfigurationAction { get; }

    private float M_max;
    private int _totalPorosity = 0;

    private RmViewModel _inputParams;

    private int _x = 0, _y = 0, _z = 0;

    private List<(VectorInt pos, float potentialDiff)> searchingCandidates
                = new List<(VectorInt pos, float potentialDiff)>();

    private List<VectorInt> clusterPositions = new List<VectorInt>();

    private List<VectorInt> takenPositions = new List<VectorInt>();

    public RmModelTest(ModelView output, ViewModel input) : base(output, input)
    {
        _inputParams = (RmViewModel)input;

        _statesNumbers.Add("solution", 0);

        _size = _inputParams.Size.X;

        _porosity = _inputParams.Porosity;

        if (_porosity != 0)
        {
            _statesNumbers.Add("nonsoluble", -1);
            _statesNumbers.Add("movable", -2);

            _initialConfigurationAction = () => CreateBoneConfig();
        }
        else 
        {
            _initialConfigurationAction = () => CreateTablet();
        }

        _cSatur = _inputParams.SaturatedConc;
        _D = _inputParams.D;
        _k = _inputParams.K;
        _dt = _inputParams.dt;
        _dx = _inputParams.dx;
        _reactProb = _inputParams.ReactionProbability;
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

        ProcessField((x, y, z) =>
        {
            var distanceSq = ((x - center.X) * (x - center.X)) +
                             ((y - center.Y) * (y - center.Y));
            _outputParameters.FieldAG[x, y, z].State =
                distanceSq <= radius * radius ? weight : StatesNumbers["solution"];
        });
    }

    private void CreateBoneConfig()
    {
        Calc();

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
                currentMass > nextMass)
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

            takenPositions.Clear();
        }
    }

    private void CreateBone()
    {
        var movableCell = GetCell(_x, _y, _z).State;

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

                    if ((float) _random.NextDouble() >= _reactProb)
                    {
                        TransformRandomSolutionNeighborToCluster();
                    }

                    CreateMovableCell();
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

    public override void Update()
    {
        if (_totalPorosity < _porosity)
        {
            CreateBone();
        }
        //else
        //{
        //    if (_outputParameters.IsFinished)
        //    {
        //        (_outputParameters as RmModelView).IsEnd = true;
        //        return;
        //    }

        //    _bufferField = new float[_inputParameters.Size.X,
        //            _inputParameters.Size.Y, _inputParameters.Size.Z];

        //    _solidCells = 0;

        //    ProcessField(ChoiceMethod);

        //    ProcessField((x, y, z) =>
        //    {
        //        GetCell(x, y, z).State += _bufferField[x, y, z];
        //    });

        //    (_outputParameters as RmModelView)._iterMass[_outputParameters.Iteration] = _releasedMass;

        //    (_outputParameters as RmModelView).SolidCells = _solidCells;

        //    _releasedMass = 0;
        //}

        base.Update();
    }

    public override bool IsFinished()
    {
        return (_outputParameters as RmModelView).SolidCells == 0;
    }
}
