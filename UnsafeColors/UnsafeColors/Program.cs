using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

internal class Program
{
    private const int WID = 500;
    private const int HGT = 500;

    [STAThread]
    private static void Main(string[] args)
    {
        Random random = new();
        Color[] colors = new Color[WID * HGT];

        Thread rayThread = new(() =>
        {
            unsafe
            {
                fixed (Color* colorPtr = &colors[0])
                {
                    RayLoop(colorPtr);
                }
            }
        });
        rayThread.Start();

        int loopCount = 0;
        Console.WriteLine("Press any key to change the color!");
        while (rayThread.IsAlive)
        {
            Console.ReadKey();
            Color color = new(random.Next(255), random.Next(255), random.Next(255), 255);
            Console.WriteLine($" {loopCount} {color.r} {color.g} {color.b}");
            for (int i = 0; i < WID; i++)
            {
                for (int j = 0; j < HGT; j++)
                {
                    int colorIdx = i * WID + j;
                    colors[colorIdx] = color;
                }
            }
            loopCount++;
        }
    }

    unsafe private static void RayLoop(Color* ptr)
    {
        SetTraceLogLevel((int)TraceLogLevel.LOG_WARNING);
        InitWindow(WID, HGT, "Unsafe Colors Demo");
        Image image = GenImageColor(WID, HGT, MAROON);
        Texture texPattern = LoadTextureFromImage(image);
        SetTextureFilter(texPattern, TextureFilter.TEXTURE_FILTER_BILINEAR);
        while (!WindowShouldClose())
        {
            BeginDrawing();
            ClearBackground(BLACK);
            UpdateTexture(texPattern, ptr);
            DrawTexture(texPattern, 0, 0, WHITE);
            EndDrawing();
        }
        CloseWindow();
    }
}
