using HegelEngine2.CellularAutomatonClass;

namespace HegelEngine2.ParametersClasses;
public class ModelView
{
    public Cell[,,] FieldAG { get; set; }
    public CellularAutomaton.AutomatonState State { get; set; }
    public bool IsFinished
    {
        get
        {
            return State == CellularAutomaton.AutomatonState.Finished ? true : false;
        }
    }
    
    public int Iteration { get; set; } = 0; 
    public TimeSpan ElapsedTime { get; set; }
    public string Log { get; set; } = string.Empty; 
   
    public override string ToString()
    {
        string outputInfo = "";
        outputInfo += $"Итерация : {Iteration}\n";
        outputInfo += $"Время на итерацию, мс : {string.Format("{0:f}", 1.0 * ElapsedTime.TotalMilliseconds / Iteration)}\n";
        outputInfo += $"Расчет окончен : {IsFinished}\n";
        outputInfo += $"Время расчета, с : {string.Format("{0:f}", ElapsedTime.TotalSeconds)}\n";

        return outputInfo;
    }
    // Просто чтобы очевиднее было, что можно лог запросить
    public string GetLog ()
    {
        return Log;
    }

    public virtual void Parse(Dictionary<string, string> parameters)
    {
        Iteration = int.Parse(parameters["Iteration"]);
    }
}
