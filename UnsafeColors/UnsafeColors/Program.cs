using Raylib_CsLo;
using System.Diagnostics;
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
    private static bool ColorsHaveUpdated = true;

    [STAThread]
    private static void Main(string[] args)
    {
        // This is the Raylib color array that will be updated
        Color[] colors = new Color[WID * HGT];

        // Load the test image data into the color array
        DemoSystemDrawing(ref colors);
        DemoImageSharp(ref colors);

        // Spawn the Raylib thrad with a pointer to the color array
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

        // Change the contents of the color array on each console key press
        int loopCount = 0;
        Random random = new();
        Console.WriteLine("Press any key to change the color!");
        while (rayThread.IsAlive)
        {
            Console.ReadKey();

            Stopwatch stopwatch = new();
            stopwatch.Start();

            Color color = new(random.Next(255), random.Next(255), random.Next(255), 255);
            for (int i = 0; i < WID * HGT; i++)
                colors[i] = color;
            
            stopwatch.Stop();
            Console.WriteLine($" Loop:{loopCount} R:{color.r} G:{color.g} B:{color.b} {stopwatch.Elapsed.TotalMilliseconds}ms");

            loopCount++;
            ColorsHaveUpdated = true;
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
            if (ColorsHaveUpdated)
            {
                UpdateTexture(texPattern, ptr);
                ColorsHaveUpdated = false;
            }
            DrawTexture(texPattern, 0, 0, WHITE);
            EndDrawing();
        }
        CloseWindow();
    }

    private static void DemoSystemDrawing(ref Color[] colors)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            Bitmap bitmap = new("test.png");
            BitmapData bmpData = bitmap.LockBits
                (new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            int numBytes = bmpData.Stride * bitmap.Height;
            byte[] bytes = new byte[numBytes];
            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(ptr, bytes, 0, numBytes);
            for (int i = 0; i < bytes.Length; i += 4)
                colors[i / 4] = new(bytes[i + 2], bytes[i + 1], bytes[i], byte.MaxValue);

            stopwatch.Stop();
            Console.WriteLine($"System.Drawing took {stopwatch.Elapsed.TotalMilliseconds}ms");
        }
        else
            Console.WriteLine("OS platform not supported");
    }

    private static void DemoImageSharp(ref Color[] colors)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>("test.png");
        byte[] bytes = new byte[WID * HGT * 4];
        image.CopyPixelDataTo(bytes);
        for (int i = 0; i < bytes.Length; i += 4)
            colors[i / 4] = new(bytes[i], bytes[i + 1], bytes[i + 2], byte.MaxValue);

        stopwatch.Stop();
        Console.WriteLine($"ImageSharp took {stopwatch.Elapsed.TotalMilliseconds}ms");
    }
}
