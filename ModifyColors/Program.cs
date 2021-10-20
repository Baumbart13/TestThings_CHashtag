using System;
using System.Drawing;
using System.IO;
using System.IO.Enumeration;

namespace ModifyColors
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap img = new Bitmap("%HOME%/Desktop/second.png");
            var mp = new ImageManipulator();

            var pixels = new Color[img.Height, img.Width];
            for (var x = 0; x < img.Width; ++x)
            {
                for (var y = 0; y < img.Height; ++y)
                {
                    pixels[x, y] = img.GetPixel(x, y);
                }
            }
            
            mp.RgbToGrayscale(ref pixels);
            mp.Otsu(ref pixels);
        }
    }
}