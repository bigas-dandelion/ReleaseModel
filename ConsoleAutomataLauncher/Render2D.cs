namespace _2DRender;

using HegelEngine2;
using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.ReleaseModel;
using HegelEngine2.Utils;
using Raylib_cs;

public class Render2D
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Scale { get; set; }

    // Порог для разделения на "твердые" и "жидкие" клетки
    private float SolidThreshold { get; init; }

    public Render2D(Cell[,,] FieldAG, int scale, ViewModel vm)
    {
        Scale = scale;
        Width = FieldAG.GetLength((int)VectorInt.Dimension.X) * Scale;
        Height = FieldAG.GetLength((int)VectorInt.Dimension.Y) * Scale;

        SolidThreshold = (vm as RmViewModel).LiquidMass;

        Raylib.InitWindow(Width, Height, "Test CARender2D");
        Raylib.SetTargetFPS(10);
    }

    public Color CalculateColor(float state)
    {
        if (state <= SolidThreshold)
        {
            byte blue = (byte)(255 - (state / SolidThreshold * 255));
            byte red = (byte)((state / SolidThreshold) * 255);
            return new Color((byte)red, (byte)0, blue, (byte)255);
        }
        else
        {
            return Color.Orange;
        }
    }

    public void Draw(Cell[,,] FieldAG, int iter)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);

        for (int y = 0; y < FieldAG.GetLength((int)VectorInt.Dimension.Y); y++)
        {
            for (int x = 0; x < FieldAG.GetLength((int)VectorInt.Dimension.X); x++)
            {
                var state = FieldAG[x, y, 0].State;

                Color c = Color.Red;

                if (state == -1)
                {
                    c = Color.Purple;
                }
                else if (state == -2)
                {
                    c = Color.Brown;
                }
                else
                {
                    c = CalculateColor(state);
                }
                
                Raylib.DrawRectangle(x * Scale, y * Scale, Scale, Scale, c);
            }
        }

        Raylib.DrawText($"FPS: {Raylib.GetFPS()}, Iteration: {iter}", 10, 10, 20, Color.Black);
        Raylib.EndDrawing();
    }
}