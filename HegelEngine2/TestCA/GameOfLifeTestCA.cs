
using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;
using HegelEngine2.Utils;

namespace HegelEngine2.TestCA;
public class GameOfLifeTestCA : CellularAutomaton
{
    public GameOfLifeTestCA(ModelView output, ViewModel input) : base(output, input)
    {
        _statesNumbers.Add("alive", 1);
    }

    public override void CreateInitialConfiguration()
    {
        (_outputParameters as GofModelView).AliveNumber = 0;
        ProcessField((x, y, z) =>
        {
            _outputParameters.FieldAG[x, y, z].State = RollAlive((_inputParameters as GofViewModel).LifeFraction)
        ? StatesNumbers["alive"]
        : StatesNumbers["empty"];
            if (GetCell(x, y, z).State == StatesNumbers["alive"]) 
                (_outputParameters as GofModelView).AliveNumber++;
        }
        );
    }
    private bool RollAlive(float probability)
    {
        return Globals.Rnd.NextDouble() <= probability;
    }
    public override bool IsFinished()
    {
        return _outputParameters.Iteration >= (_inputParameters as GofViewModel).Iterations;
    }


    public override void Update()
    {
        if (_outputParameters.IsFinished)
        {            
            return;
        }

        int alive = 0;
        Cell[,,] bufferField = new Cell[
            _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.X),
            _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Y),
            _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Z)];

        ProcessField((x, y, z) =>
        {
            int liveNeighbors = 0;
            var checkPos = new VectorInt(0, 0, 0);
            foreach (var n in _neighbors)
            {
                checkPos.X = x + n.X;
                checkPos.Y = y + n.Y;
                checkPos.Z = z + n.Z;

                ProcessBorder(checkPos.X, checkPos.Y, checkPos.Z, out float state);
                if (state == StatesNumbers["alive"]) liveNeighbors++;
            }
            if (liveNeighbors == 3)
            {
                bufferField[x, y, z] = new Cell(x, y, z, StatesNumbers["alive"]);
            }
            else if (liveNeighbors == 2)
            {
                bufferField[x, y, z] = new Cell(x, y, z, GetCell(x, y, z).State);
            }
            else
            {
                bufferField[x, y, z] = new Cell(x, y, z, StatesNumbers["empty"]);
            }
        });
        ProcessField((x, y, z) =>
        {
            GetCell(x, y, z).State = bufferField[x, y, z].State;
            if (GetCell(x, y, z).State == StatesNumbers["alive"]) alive++;
        }
        );

        (_outputParameters as GofModelView).AliveNumber = alive;
        base.Update();        
    }
}
