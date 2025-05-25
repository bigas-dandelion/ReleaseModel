using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;

namespace HegelEngine2.DLA;
public class DLAModelView : ModelView
{
    public DLAModelView() : base()
    {

    }

    public bool IsEnd { get; set; }

    public float SolidCells { get; set; }

    public override string ToString()
    {
        var output = base.ToString();
        output += $"Кол-во твёрдых клеток: {SolidCells}\n";
        return output;
    }
}