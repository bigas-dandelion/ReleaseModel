using _2DRender;
using HegelEngine2;
using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;
using HegelEngine2.ReleaseModel;
using Raylib_cs;
using System.Text;

namespace ConsoleAutomataLauncher;

internal class Program
{
    private static void Main()
    {
        ModelView mv = new RmModelView();

        ViewModel vm = new RmViewModel();
        vm.BoundaryConditions = CellularAutomaton.BoundaryConditions.Bounce;
        (vm as RmViewModel).Iterations = 100;
        (vm as RmViewModel).Name = ViewModel.AutomataName.ReleaseModel;
        (vm as RmViewModel).Size = (150, 150, 1);

        (vm as RmViewModel).Diameter = 32f;
        (vm as RmViewModel).SolidMass = 23f;

        (vm as RmViewModel).SaturatedConc = 32f;
        (vm as RmViewModel).dx = 0.1f;
        (vm as RmViewModel).dt = 0.1f;
        (vm as RmViewModel).D = 0.001f;
        (vm as RmViewModel).K = 0.0001f;

        CellularAutomaton relModel = new RmModelTest(mv, vm);
        relModel.InitializeAutomata();

        Render2D render = new Render2D(mv.FieldAG, 6, vm);

        while (!Raylib.WindowShouldClose())
        {
            if ((mv as RmModelView).IsEnd)
            {
                Thread.Sleep(3000);
                DataToExcel(mv);
                Raylib.CloseWindow();
                break;
            }

            relModel.Update();
            render.Draw(mv.FieldAG, mv.Iteration);
        }
    }

    private static void DataToExcel(ModelView mv)
    {
        string timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        string csvFilePath = $"output_{timestamp}.csv";

        using (var writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
        {
            writer.WriteLine("№ итерации;Масса жидких");

            foreach (var kvp in (mv as RmModelView)._iterMass.OrderBy(k => k.Key))
            {
                string formattedValue = kvp.Value.ToString().Replace('.', ',');
                writer.WriteLine($"{kvp.Key};{formattedValue}");
            }
        }

        Console.WriteLine($"CSV-файл сохранён: {Path.GetFullPath(csvFilePath)}");
    }
}