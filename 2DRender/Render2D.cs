namespace _2DRender;
using Raylib_cs;
using System.Runtime.CompilerServices;

public class Render2D
{
    public void Draw ()
    {
        Raylib.InitWindow(width * cellSize, height * cellSize, "Game of Life (Raylib-C#)");
        Raylib.SetTargetFPS(10);

        RandomizeGrid();

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y])
                        Raylib.DrawRectangle(x * cellSize, y * cellSize, cellSize, cellSize, Color.Black);
                    else
                        Raylib.DrawRectangleLines(x * cellSize, y * cellSize, cellSize, cellSize, Color.LightGray);
                }
            }

            Raylib.DrawText(isPaused ? "PAUSED (SPACE to play)" : "RUNNING (SPACE to pause)", 10, 10, 20, Color.Red);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
