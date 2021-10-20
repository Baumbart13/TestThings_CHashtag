using System.Drawing;
using System.Drawing.Imaging;

namespace ModifyColors
{
    public class ImageManipulator
    {
        public void RgbToGrayscale(ref Color[,] pixels)
        {
            for (var i = 0; i < pixels.GetLength(0); ++i)
            {
                for (var j = 0; j < pixels.GetLength(1); ++j)
                {
                    var p = pixels[i, j];
                    var gray = (p.R + p.G + p.B) / 3;
                    
                    pixels[i, j] = Color.FromArgb(gray, gray, gray);
                }
            }
        }
        
        public int Otsu(ref Bitmap bmp)
        {
            byte t=0;
            float[] vet=new float[256];
            int[] hist=new int[256];
            vet.Initialize();

            float p1,p2,p12;
            int k;

            var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0.ToPointer();

                getHistogram(p,bmp.Width,bmp.Height,bmData.Stride, hist);

        
                for (k = 1; k != 255; k++)
                {
                    p1 = Px(0, k, hist);
                    p2 = Px(k + 1, 255, hist);
                    p12 = p1 * p2;
                    if (p12 == 0) 
                        p12 = 1;
                    float diff=(Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                    vet[k] = (float)diff * diff / p12;
            
                }
            }
            bmp.UnlockBits(bmData);

            t = (byte)findMax(vet, 256);

            return t;
        }
    }
}