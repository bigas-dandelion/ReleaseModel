using _2DRender;
using HegelEngine2;
using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;
using HegelEngine2.ReleaseModel;
using Raylib_cs;

namespace ConsoleAutomataLauncher;

internal class Program
{
    private static void Main()
    {
        ModelView mv = new RmModelView();

        ViewModel vm = new RmViewModel();
        vm.BoundaryConditions = CellularAutomaton.BoundaryConditions.Bounce;
        (vm as RmViewModel).Iterations = 1000;
        (vm as RmViewModel).Name = ViewModel.AutomataName.ReleaseModel;
        (vm as RmViewModel).Size = (300, 300, 1);
        (vm as RmViewModel).Diameter = 32f;
        (vm as RmViewModel).SolidMass = 23f;
        (vm as RmViewModel).LiquidMass = 12f;
        (vm as RmViewModel).D = 0.001f;
        (vm as RmViewModel).K = 0.0001f;

        CellularAutomaton relModel = new RmModelTest(mv, vm);
        relModel.InitializeAutomata();

        Render2D render = new Render2D(mv.FieldAG, 3);

        while (!Raylib.WindowShouldClose())
        {
            if (File.Exists("output.txt"))
            {
                Thread.Sleep(3000);
                break;
            }

            relModel.Update();
            render.Draw(mv.FieldAG);
        }

        Raylib.CloseWindow();

        DataToExcel();
    }

    private static void DataToExcel()
    {
        string inputPath = "output.txt";
        string outputPath = "output.csv";

        string[] lines = File.ReadAllLines(inputPath);

        using (var writer = new StreamWriter(outputPath, false, new System.Text.UTF8Encoding(true)))
        {
            writer.WriteLine("№ итерации;Масса жидких");

            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length != 2) continue;

                string index = parts[0].Trim();
                string value = parts[1].Trim().Replace(".", ",");

                writer.WriteLine($"{index};{value}");
            }
        }

        Console.WriteLine($"CSV-файл сохранён: {Path.GetFullPath(outputPath)}");
    }
}