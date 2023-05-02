using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

internal class Program
{
    private const int WID = 500;
    private const int HGT = 500;

    [STAThread]
    private static void Main(string[] args)
    {
        Color[] colors = new Color[WID * HGT];

        new Thread(() =>
        {
            unsafe
            {
                fixed (Color* colorPtr = &colors[0])
                {
                    RayLoop(colorPtr);
                }
            }
        }).Start();


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
