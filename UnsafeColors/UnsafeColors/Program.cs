using Raylib_CsLo;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static Raylib_CsLo.Raylib;
using Color = Raylib_CsLo.Color;
using Image = Raylib_CsLo.Image;

internal class Program
{
    private const int WID = 500;
    private const int HGT = 500;

    [STAThread]
    private static void Main(string[] args)
    {
        Color[] colors = new Color[WID * HGT];

        byte[] bytes = DemoWindowsBitmapByteArray();
        for (int i = 0; i < bytes.Length; i += 4)
            colors[i / 4] = new(bytes[i + 2], bytes[i + 1], bytes[i], byte.MaxValue);

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
        Random random = new();
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

    private static byte[] DemoWindowsBitmapByteArray()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Bitmap bitmap = new("test.png");
            BitmapData bmpData = bitmap.LockBits
                (new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            int numbytes = bmpData.Stride * bitmap.Height;
            byte[] bytedata = new byte[numbytes];
            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(ptr, bytedata, 0, numbytes);
            return bytedata;
        }
        Console.WriteLine("Not supported");
        return new byte[WID * HGT];
    }
}
