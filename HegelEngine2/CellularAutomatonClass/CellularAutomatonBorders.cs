using HegelEngine2.Utils;

namespace HegelEngine2.CellularAutomatonClass;

public abstract partial class CellularAutomaton
{
    public bool AreBordersCrossed(VectorInt point)
    {
        return AreBordersCrossed(point.X, point.Y, point.Z);
    }

    public bool AreBordersCrossed(int x, int y, int z)
    {
        return FindBordersCrosses(x, y, z).Any(b => b.Value == true);
    }

    public VectorInt? ProcessBorder(VectorInt p, out float state)
    {
        return ProcessBorder(p.X, p.Y, p.Z, out state);
    }
    public VectorInt? ProcessBorder(int x, int y, int z, out float state)
    {
        VectorInt? outputCell = null;
        state = StatesNumbers["border"];
        switch (_inputParameters.BoundaryConditions)
        {
            case BoundaryConditions.Bounce:
            default:
                {
                    if (AreBordersCrossed(x, y, z) == false)
                    {
                        outputCell = new VectorInt(x, y, z);
                        state = GetCell(x, y, z).State;
                    }
                    break;
                }
            case BoundaryConditions.Periodic:
                {
                    x = (x + _inputParameters.Size.X) % _inputParameters.Size.X;
                    y = (y + _inputParameters.Size.Y) % _inputParameters.Size.Y;
                    z = (z + _inputParameters.Size.Z) % _inputParameters.Size.Z;
                    outputCell = new VectorInt(x, y, z);
                    outputCell.X = x;
                    outputCell.Y = y;
                    outputCell.Z = z;
                    state = GetCell(x, y, z).State;
                    break;
                }
        }
        return outputCell;
    }

    public Dictionary<Boundary, bool> FindBordersCrosses(VectorInt point)
    {
        return FindBordersCrosses(point.X, point.Y, point.Z);
    }
    public Dictionary<Boundary, bool> FindBordersCrosses(int x, int y, int z)
    {
        Dictionary<Boundary, bool> crosses = new Dictionary<Boundary, bool>()
        {
            {Boundary.PositiveX, false},
            {Boundary.NegativeX, false},
            {Boundary.PositiveY, false},
            {Boundary.NegativeY, false},
            {Boundary.PositiveZ, false},
            {Boundary.NegativeZ, false},
        };

        if (x < 0)
            crosses[Boundary.NegativeX] = true;
        else if (x >= _inputParameters.Size.X)
            crosses[Boundary.PositiveX] = true;

        if (y < 0)
            crosses[Boundary.NegativeY] = true;
        else if (y >= _inputParameters.Size.Y)
            crosses[Boundary.PositiveY] = true;

        if (Is3D())
        {
            if (z < 0)
                crosses[Boundary.NegativeZ] = true;
            else if (z >= _inputParameters.Size.Y)
                crosses[Boundary.PositiveZ] = true;
        }
        return crosses;
    }
    public enum Boundary : byte
    {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ,
    }

    public enum BoundaryConditions : byte
    {
        Bounce,
        Periodic
    }

}

