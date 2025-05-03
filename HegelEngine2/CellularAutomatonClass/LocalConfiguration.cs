using HegelEngine2.Utils;

namespace HegelEngine2.CellularAutomatonClass;

public class LocalConfiguration
{
    public string Id { get; }
    public List<VectorInt> CellsPos { get; }
    public List<VectorInt> NeighborsPos { get; }
    public LocalConfiguration()
    {
        CellsPos = new List<VectorInt>();
        NeighborsPos = new List<VectorInt>();
        Id = Guid.NewGuid().ToString();
    }
    public virtual void InitializeConfiguration()
    {
        CellsPos.Add(new VectorInt(0, 0));
    }
    public virtual void Transform(VectorInt v)
    {
        //CellsPos.ForEach(c => c += v);
        //NeighborsPos.ForEach(n => n += v);
        for (int i = 0; i < CellsPos.Count; i++)
        {
            CellsPos[i] += v;
        }
        for (int i = 0; i < NeighborsPos.Count; i++)
        {
            NeighborsPos[i] += v;
        }
    }

}
