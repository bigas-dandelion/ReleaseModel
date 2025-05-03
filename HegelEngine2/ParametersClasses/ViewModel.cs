using HegelEngine2.CellularAutomatonClass;
namespace HegelEngine2;
public class ViewModel
{
    public AutomataName Name { get; set; }
    public (int X, int Y, int Z) Size { get; set; } = (100, 100, 1);
    // Не null, если поле загружено извне
    public float[,,]? InputField { get; set; } = null;
    public byte NeighborhoodOrder { get; set; } = 1;
    public CellularAutomaton.NeighborType Neighborhood { get; set; } = CellularAutomaton.NeighborType.Moore;
    public CellularAutomaton.BoundaryConditions BoundaryConditions { get; set; } = CellularAutomaton.BoundaryConditions.Bounce;
    public static Dictionary<int, CellularAutomaton.NeighborType> NeighborsType { get; } =
        new Dictionary<int, CellularAutomaton.NeighborType>()
        {
            {1, CellularAutomaton.NeighborType.Neiman },
            {2, CellularAutomaton.NeighborType.Moore },
        };

    public static Dictionary<int, CellularAutomaton.BoundaryConditions> BoundariesTypes { get; } =
        new Dictionary<int, CellularAutomaton.BoundaryConditions>()
        {
            {1, CellularAutomaton.BoundaryConditions.Bounce },
            {2, CellularAutomaton.BoundaryConditions.Periodic },
        };
    public static Dictionary<string, AutomataName> AutomataNames { get; } = new ()
        {
            {"Game of Life", AutomataName.GameOfLife },
            {"Particle Cluster Aggregation", AutomataName.DLA },
            {"Cluster Cluster Aggregation", AutomataName.DLCA },
            {"Dissolution Model", AutomataName.Dissolution },
            {"Membrane Dissolution Model", AutomataName.MembraneDissolution },
            {"Bezier Curves", AutomataName.BezierCurves },
            {"Inverted ReversedRLA", AutomataName.InvertedReversedRLA },
            {"Langton Ant", AutomataName.LangtonAnt },
            {"Release Model", AutomataName.ReleaseModel },
        };

    public override string ToString()
    {
        string parameters = string.Empty;
        parameters += $"Name={AutomataNames.First(n => n.Value == Name).Key}\n";
        parameters += $"Size={Size.X};{Size.Y};{Size.Z}\n";
        parameters += $"NeighborhoodOrder={NeighborhoodOrder}\n";
        parameters += $"Neighborhood={NeighborsType.First(n => n.Value==Neighborhood).Key}\n"; // Не, это хреново, но пусть хоть так.
        parameters += $"BoundaryConditions={BoundariesTypes.First(n => n.Value == BoundaryConditions).Key}\n";
        return parameters;
    }

    public virtual void Parse (Dictionary<string, string> parameters)
    {
        if (parameters.ContainsKey("Name"))
            Name = AutomataNames[parameters["Name"]];
        var dimensions = new string [3];
        if (parameters.ContainsKey("Size"))
        {
            dimensions = parameters["Size"].Split(';');
            Size = (int.Parse(dimensions[0]), int.Parse(dimensions[1]), int.Parse(dimensions[2]));
        }            
        NeighborhoodOrder = byte.Parse(parameters["NeighborhoodOrder"]);
        Neighborhood = NeighborsType[int.Parse(parameters["Neighborhood"])];
        BoundaryConditions = BoundariesTypes[int.Parse(parameters["BoundaryConditions"])];
    }
    public enum AutomataName : byte
    {
        GameOfLife,
        Dissolution,
        MembraneDissolution,
        DLA,
        DLCA,
        BezierCurves,
        InvertedReversedRLA,
        LangtonAnt,
        ReleaseModel,
    }
}
