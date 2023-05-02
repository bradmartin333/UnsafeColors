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
    private static bool ColorsHaveUpdated = true;

    [STAThread]
    private static void Main(string[] args)
    {
        // This is the Raylib color array that will be updated
        Color[] colors = new Color[WID * HGT];

        // Load the test image data into the color array
        byte[] bytes = DemoWindowsBitmapByteArray();
        for (int i = 0; i < bytes.Length; i += 4)
            colors[i / 4] = new(bytes[i + 2], bytes[i + 1], bytes[i], byte.MaxValue);

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
            Color color = new(random.Next(255), random.Next(255), random.Next(255), 255);
            Console.WriteLine($" {loopCount} {color.r} {color.g} {color.b}");
            for (int i = 0; i < WID * HGT; i++)
                colors[i] = color;
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

    /// <summary>
    /// Standard Windows BitmapData extraction
    /// </summary>
    /// <returns>32bppRgb byte array of test.png</returns>
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
        Console.WriteLine("OS platform not supported");
        return new byte[WID * HGT];
    }
}
