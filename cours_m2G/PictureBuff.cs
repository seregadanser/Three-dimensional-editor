using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Xml.Linq;
using System.Runtime.InteropServices;

namespace cours_m2G
{
    static class PictureBuff
    {
        public static int[] rgb;
        public static Semaphore sem;
        static Graphics g;
        public static Bitmap bmp;
        static Size screen;
        public static Size Screen { get { return screen; } set { screen = value; bmp = new Bitmap(screen.Width, screen.Height); rgb = new int[screen.Width * screen.Height];
                for (int x = 0; x < screen.Width; x++)
                    for (int y = 0; y < screen.Height; y++)
                            rgb[x + y * screen.Width] = -1;

            } }
        static bool filled;
       public static object locker = new();
        static public RenderType Creator { get; set; } = RenderType.NOCUTTER;
        public static bool Filled { get { return filled; } set { filled = value; /*if (value) r.Invoke();*/} } 
        public static void Init(Size screen)
        {


            PictureBuff.screen = screen;
            bmp = new Bitmap(screen.Width, screen.Height);
            g = Graphics.FromImage(bmp);
            rgb = new int[screen.Width * screen.Height];
            for (int x = 0; x < screen.Width; x++)
                for (int y = 0; y < screen.Height; y++)
                    rgb[x+ y*screen.Width] = -1;
  
            filled = false;
        
        }

        public static void SetPixel(int x, int y, int color)
        {
            try
            {
                rgb[x + y * screen.Width] = color;
            }
            catch { }
        }
        public static void SetLine(int x1, int y1, int x2, int y2,Color color)
        {
            Pen pen = new Pen(color, 4);
            lock (locker)
                try
                {
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
                catch { }
           

        }
        public static void SetPoint(MatrixCoord3D p1, int HitRadius,Color color)
        {
            Pen pen = new Pen(color);
            lock (locker)
                try
                {
                    g.DrawEllipse(pen, (int)(p1.X - HitRadius), (int)(p1.Y - HitRadius), HitRadius * 2, HitRadius * 2);
                }
                catch { }
           
        }

        public static void SetText(MatrixCoord3D p1, string s, string s1)
        {
            g.DrawString(s, new Font("Arial", 8), new SolidBrush(Color.Black), (int)p1.X, (int)p1.Y);
            g.DrawString(s1, new Font("Arial", 8), new SolidBrush(Color.Blue), (int)p1.X, (int)p1.Y + 11);

          
        }
        public static Bitmap BuildImage(Byte[] sourceData, Int32 width, Int32 height, Int32 stride, PixelFormat pixelFormat, Color[] palette, Color? defaultColor)
        {
            Bitmap newImage = new Bitmap(width, height, pixelFormat);
            BitmapData targetData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newImage.PixelFormat);
            Int32 newDataWidth = ((Image.GetPixelFormatSize(pixelFormat) * width) + 7) / 8;
            // Compensate for possible negative stride on BMP format.
            Boolean isFlipped = stride < 0;
            stride = Math.Abs(stride);
            // Cache these to avoid unnecessary getter calls.
            Int32 targetStride = targetData.Stride;
            Int64 scan0 = targetData.Scan0.ToInt64();
            for (Int32 y = 0; y < height; y++)
                Marshal.Copy(sourceData, y * stride, new IntPtr(scan0 + y * targetStride), newDataWidth);
            newImage.UnlockBits(targetData);
            // Fix negative stride on BMP format.
            if (isFlipped)
                newImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            // For indexed images, set the palette.
            if ((pixelFormat & PixelFormat.Indexed) != 0 && palette != null)
            {
                ColorPalette pal = newImage.Palette;
                for (Int32 i = 0; i < pal.Entries.Length; i++)
                {
                    if (i < palette.Length)
                        pal.Entries[i] = palette[i];
                    else if (defaultColor.HasValue)
                        pal.Entries[i] = defaultColor.Value;
                    else
                        break;
                }
                newImage.Palette = pal;
            }
            return newImage;
        }


        public static Bitmap GetBitmap()
        {
            
            if (Creator != RenderType.NOCUTTER)
            {
                //for (int x = 0; x < screen.Width; x++)
                //    for (int y = 0; y < screen.Height; y++)
                //        bmp.SetPixel(x, y, Color.FromArgb(rgb[x + y * screen.Width]));

                Int32 width = screen.Width;
                Int32 height = screen.Height;
                Int32 stride = width * 4;
                Int32 byteIndex = 0;
                Byte[] dataBytes = new Byte[height * stride];
                for (Int32 y = 0; y < height; y++)
                {
                    for (Int32 x = 0; x < width; x++)
                    {
                        // UInt32 0xAARRGGBB = Byte[] { BB, GG, RR, AA }
                        UInt32 val = (UInt32)rgb[x + y * screen.Width];
  
                        // This code clears out everything but a specific part of the value
                        // and then shifts the remaining piece down to the lowest byte
                        dataBytes[byteIndex + 0] = (Byte)(val & 0x000000FF); // B
                        dataBytes[byteIndex + 1] = (Byte)((val & 0x0000FF00) >> 08); // G
                        dataBytes[byteIndex + 2] = (Byte)((val & 0x00FF0000) >> 16); // R
                        dataBytes[byteIndex + 3] = (Byte)((val & 0xFF000000) >> 24); // A
                                                                                     // More efficient than multiplying
                        byteIndex += 4;
                    }
                }
                bmp = BuildImage(dataBytes, width, height, stride, PixelFormat.Format32bppArgb, null, null);
            }
            return bmp;
        }
        public static void Clear()
        {
            if (Creator != RenderType.NOCUTTER)
            {
                rgb = new int[screen.Width* screen.Height];
                //for (int x = 0; x < screen.Width; x++)
                //    for (int y = 0; y < screen.Height; y++)
                //        rgb[x + y * screen.Width] = -1;
            }
            else
            {
               g.Clear(Color.White);
            }
        }
    }
}
