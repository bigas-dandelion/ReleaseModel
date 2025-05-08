using HegelEngine2.ParametersClasses;

namespace HegelEngine2.ReleaseModel;

public class RmModelView : ModelView
{
    public RmModelView() : base()
    {

    }

    public bool IsEnd { get; set; }

    public Dictionary<int, float> _iterMass = new Dictionary<int, float>();

    public float SolidCells { get; set; }

    public override string ToString()
    {
        var output = base.ToString();
        output += $"Кол-во твёрдых клеток: {SolidCells}\n";
        return output;
    }
}