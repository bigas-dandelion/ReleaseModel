using HegelEngine2.ParametersClasses;
using HegelEngine2.Utils;

namespace HegelEngine2.ReleaseModel;

public class RmModelView : ModelView
{
    public RmModelView() : base()
    {

    }

    public bool IsEnd { get; set; }

    public Dictionary<int, float> _iterMass = new Dictionary<int, float>();

    //public List<VectorInt> clusterPositions = new List<VectorInt>();

    public HashSet<(int x, int y, int z)> ClusterCoords3d = new();

    public float SolidCells { get; set; }

    public override string ToString()
    {
        var output = base.ToString();
        output += $"Кол-во твёрдых клеток: {SolidCells}\n";
        return output;
    }
}