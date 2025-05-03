namespace _2DRender;

using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.Utils;
using Raylib_cs;

public class Render2D
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Scale { get; set; }

    // Порог для разделения на "твердые" и "жидкие" клетки
    public float SolidThreshold { get; set; } = 0.5f;

    public Render2D(Cell[,,] FieldAG, int scale)
    {
        Scale = scale;
        Width = FieldAG.GetLength((int)VectorInt.Dimension.X) * Scale;
        Height = FieldAG.GetLength((int)VectorInt.Dimension.Y) * Scale;
        Raylib.InitWindow(Width, Height, "Test CARender2D");
        Raylib.SetTargetFPS(10);
    }

    public Color CalculateColor(float state)
    {
        if (state <= SolidThreshold)
        {
            byte blue = (byte)(50 + (state / SolidThreshold) * 205);
            return new Color((byte)0, (byte)0, blue, (byte)255);
        }
        else
        {
            byte red = (byte)(50 + ((state - SolidThreshold) / (1f - SolidThreshold)) * 205);
            return new Color(red, (byte)0, (byte)0, (byte)255);
        }
    }

    public void Draw(Cell[,,] FieldAG)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);

        for (int y = 0; y < FieldAG.GetLength((int)VectorInt.Dimension.Y); y++)
        {
            for (int x = 0; x < FieldAG.GetLength((int)VectorInt.Dimension.X); x++)
            {
                var c = CalculateColor(FieldAG[x, y, 0].State);

                Raylib.DrawRectangle(x * Scale, y * Scale, Scale, Scale, c);

                //        float red = 0f;
                //        float blue = 0f;

                //        if (FieldAG[x, y, 0].State < 12f)
                //        {
                //            blue = FieldAG[x, y, 0].State;
                //        }
                //        else 
                //        {
                //            red = FieldAG[x, y, 0].State;
                //        }

                //        Color c = new Color(
                //            r: red,
                //            g: 0,
                //            b: blue,
                //            a: 255
                //        );

                //        Raylib.DrawRectangle(x * Scale, y * Scale, Scale, Scale, c);
                //    }
                //}
            }
        }

        Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 10, 10, 20, Color.Black);
        Raylib.EndDrawing();
    }
}