using _2DRender;
using HegelEngine2;
using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ParametersClasses;
using HegelEngine2.ReleaseModel;
using HegelEngine2.DLA;
using Raylib_cs;
using System.Text;

namespace ConsoleAutomataLauncher;

internal class Program
{
    private static void Main()
    {
        bool isTherePorosity = true;

        string file = "try3";

        CellularAutomaton relModel;
        ModelView mv;
        ViewModel vm;

        if (isTherePorosity)
        {
            var dlaVm = new DLAViewModel
            {
                BoundaryConditions = CellularAutomaton.BoundaryConditions.Bounce,
                Name = ViewModel.AutomataName.DLA,
                Size = (150, 150, 1),
                SolidMass = 23f,
                BootParam = 55,
                Diameter = 32f,
                Porosity = 45,
                ReactionProbability = 0.12f,
                FileName = file
            };

            vm = dlaVm;
            mv = new DLAModelView();
            relModel = new DLATest(mv, vm);
            relModel.InitializeAutomata();

            while (!(mv as DLAModelView).IsEnd)
            {
                relModel.Update();
            }
        }

        var rmVm = new RmViewModel
        {
            BoundaryConditions = CellularAutomaton.BoundaryConditions.Bounce,
            Name = ViewModel.AutomataName.ReleaseModel,
            FileName = file,
            Size = (150, 150, 1),
            Diameter = 21f,
            SolidMass = 23f,
            SaturatedConc = 32f,
            dx = 0.1f,
            dt = 0.1f,
            D = 0.001f,
            K = 0.01f,
            IsTherePorosity = isTherePorosity
        };

        vm = rmVm;
        mv = new RmModelView();
        relModel = new RmModelTest(mv, vm);
        relModel.InitializeAutomata();

        var render = new Render2D(vm.InputField, 6, vm);

        while (!Raylib.WindowShouldClose())
        {
            if ((mv as RmModelView).IsEnd)
            {
                Thread.Sleep(3000);
                //DataToExcel(mv);
                Raylib.CloseWindow();
                break;
            }

            relModel.Update();
            render.DrawInputField(vm.InputField, mv.Iteration);
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