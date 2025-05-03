using System.Diagnostics;
using HegelEngine2.CellularAutomatonClass;

namespace HegelEngine2.Utils;

public class Presenter
{
    private IView _view;
    private CellularAutomaton _cellularAutomaton;
    private bool _active = false;
    private bool _isSingleStep = false;
    private Stopwatch _stopwatch;
    public Presenter(IView view, CellularAutomaton cellularAutomaton)
    {
        _view = view;
        _cellularAutomaton = cellularAutomaton;

        _view.InitializationBegan += ViewModelInitialize;
        _view.Launched += ViewModelLaunch;
        _view.CellsChanged += ViewModelChangeCells;


        _cellularAutomaton.Initialized += ModelViewUpdate;
        _cellularAutomaton.Updated += ModelViewUpdate;
        _cellularAutomaton.Finished += ModelViewUpdate;
    }

    public void Dispose()
    {
        _view.InitializationBegan -= ViewModelInitialize;
        _view.Launched -= ViewModelLaunch;
        _view.CellsChanged -= ViewModelChangeCells;
        _view = null;

        _cellularAutomaton.Initialized -= ModelViewUpdate;
        _cellularAutomaton.Updated -= ModelViewUpdate;
        _cellularAutomaton.Finished -= ModelViewUpdate;
        _cellularAutomaton = null;
    }

    private void ViewModelInitialize(object? sender, EventArgs e)
    {

        _cellularAutomaton.InitializeAutomata();
        //_cellularAutomaton.OnInitialized();
        _stopwatch = new Stopwatch();
        _active = false;
    }
    //private void ViewModelLoadingInitialize()
    //{
    //    _cellularAutomaton.InitializeAutomata();
    //    _cellularAutomaton.OnInitialized();
    //    _stopwatch = new Stopwatch();
    //    _active = false;
    //}
    private async void ViewModelLaunch(object? sender, EventArgs e)
    {
        if (_isSingleStep)
        {
            if (_view.InputParameters.State == CellularAutomaton.AutomatonState.Ready ||
                _view.InputParameters.State == CellularAutomaton.AutomatonState.Processing)
                _cellularAutomaton.Update();
        }
        else
        {
            //TODO: Как лучше - делать одну таску или создавать новую под каждую итерацию как сейчас?
            _active = !_active;

            await Task.Run(() =>
            {
                while (_view.InputParameters.State == CellularAutomaton.AutomatonState.Ready ||
               _view.InputParameters.State == CellularAutomaton.AutomatonState.Processing ||
               _view.InputParameters.State == CellularAutomaton.AutomatonState.Paused)
                {
                    if (_active == false)
                    {
                        _view.InputParameters.State = CellularAutomaton.AutomatonState.Paused;
                        _view.Update();
                        return;
                    }
                    else
                    {
                        _stopwatch.Start();
                        _cellularAutomaton.Update();
                        _view.InputParameters.ElapsedTime = _stopwatch.Elapsed;
                    }
                }
            });
        }
    }
    private void ViewModelChangeCells(object? sender, CellStateChangingEventArgs e)
    {
        foreach (var c in e.newStates)
        {
            _cellularAutomaton.ChangeState(c.state, c.pos);
        }
    }

    private void ModelViewUpdate()
    {
        _view.Update();
        if (_view.InputParameters.State == CellularAutomaton.AutomatonState.Finished)
        {
            _stopwatch.Stop();
            _active = false;
        }
    }
}
