namespace _2DRender;

using HegelEngine2;
using HegelEngine2.CellularAutomatonClass;
using HegelEngine2.DLA;
using HegelEngine2.ParametersClasses;
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

    //public List<VectorInt>? ClusterPositions;

    public HashSet<(int x, int y, int z)> ClusterCoords3d;

    private bool isCluster;

    public Render2D(Cell[,,] FieldAG, int scale, ViewModel vm, ModelView mv)
    {
        Scale = scale;
        Width = FieldAG.GetLength((int)VectorInt.Dimension.X) * Scale;
        Height = FieldAG.GetLength((int)VectorInt.Dimension.Y) * Scale;

        SolidThreshold = (vm as RmViewModel).LiquidMass;

        ClusterCoords3d = (mv as RmModelView).ClusterCoords3d;

        //ClusterPositions = (mv as RmModelView).clusterPositions;

        //isCluster = ClusterPositions != null;

        Raylib.InitWindow(Width, Height, "Test CARender2D");
        Raylib.SetTargetFPS(10);
    }

    public Color CalculateColor(float state, bool isCluster)
    {
        if (isCluster)
        {
            byte blue = (byte)(125 - (state / SolidThreshold * 125));
            byte red = (byte)(125 - (state / SolidThreshold * 125));
            byte green = (byte)(125 - (state / SolidThreshold * 125));
            return new Color(red, green, blue, (byte)255);
        }

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
                Color c = Color.Red;

                var state = FieldAG[x, y, 0].State;

                isCluster = ClusterCoords3d.Contains((x, y, 0)) ? true : false;

                //isCluster = ClusterPositions.Any(v => v.X == x && v.Y == y) ? true : false;

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
                    c = CalculateColor(state, isCluster);
                }
                
                Raylib.DrawRectangle(x * Scale, y * Scale, Scale, Scale, c);
            }
        }

        Raylib.DrawText($"FPS: {Raylib.GetFPS()}, Iteration: {iter}", 10, 10, 20, Color.Black);
        Raylib.EndDrawing();
    }
}