using HegelEngine2.Utils;

namespace HegelEngine2.CellularAutomatonClass;

public class Cell
{
    public string? LocalConfigurationId { get; private set; } = null;
    public VectorInt Pos { get; }
    public float State { get; set; }

    public List<string> Tags = new List<string>();

    public Cell(VectorInt pos, float state)
    {
        Pos = pos;
        State = state;
    }

    public Cell(int x, int y, int z, float state)
    {
        Pos = new VectorInt(x, y, z);
        State = state;
    }

    public Cell(VectorInt pos, float state, string id) : this(pos, state)
    {
        ChangeLocalConfiguration(id);
    }
    public void ChangeLocalConfiguration(string id)
    {
        LocalConfigurationId = id;
    }
    public void DeleteLocalConfiguration()
    {
        LocalConfigurationId = null;
    }
    public override string ToString()
    {
        string output = $"Координата: {Pos}\nСостояние: {State}";
        // Я пробовал писать в одну строку через тернарый оператор, но он тогда отображает некорректно. Хз, почему
        if (LocalConfigurationId != null)
            output += $"\nКонфигурация: {LocalConfigurationId}";
        return output;
    }
}