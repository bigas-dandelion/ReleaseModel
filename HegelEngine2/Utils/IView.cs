using HegelEngine2.ParametersClasses;

namespace HegelEngine2.Utils;
public interface IView
{
    ModelView? InputParameters { get; }
    ViewModel? OutputParameters { get; }


    event EventHandler? InitializationBegan;
    event EventHandler? Launched;
    event EventHandler<CellStateChangingEventArgs>? CellsChanged;

    void Update();

}

public class CellStateChangingEventArgs : EventArgs
{
    public List<(VectorInt pos, string state)> newStates { get; set; } = new();
}


