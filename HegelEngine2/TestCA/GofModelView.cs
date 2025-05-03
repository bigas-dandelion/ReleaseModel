using HegelEngine2.ParametersClasses;

namespace HegelEngine2.TestCA;

public class GofModelView : ModelView
{
    public GofModelView() : base()
    {
       
    }
    public int AliveNumber { get; set; }
    public override string ToString()
    {
        var output = base.ToString();
        output += $"Кол-во живых клеток: {AliveNumber}\n";
        return output;
    }
}
