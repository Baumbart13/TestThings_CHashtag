using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ModifyColors
{
    public class ImageManipulator
    {
        public const int INTENSITY_LAYER_NUMBER = 256;

        public void RgbToGrayscale(ref Color[,] pixels, ref Bitmap img)
        {
            for (var i = 0; i < pixels.GetLength(0); ++i)
            {
                for (var j = 0; j < pixels.GetLength(1); ++j)
                {
                    var p = img.GetPixel(i, j);
                    var gray = (p.R + p.G + p.B) / 3;

                    pixels[i, j] = Color.FromArgb(gray, gray, gray);
                }
            }
        }

        private int[] colors2dToInt1d(ref Color[,] colors)
        {
            var pixels = new int[colors.Length];
            var counter = 0;
            foreach (var i in colors)
            {
                pixels[counter++] = i.B;
            }

            return pixels;
        }

        private void calculateHistogram(ref int[] pixels, int pixelCount, ref int[] hist)
        {
            Array.Clear(hist, 0, hist.Length);

            for (var i = 0; i < pixelCount; ++i)
            {
                ++hist[pixels[i]];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="pixelCount"></param>
        /// <returns></returns>
        private int calculateIntensitySum(ref int[] pixels, int pixelCount)
        {
            var sum = 0;
            foreach (var pixel in pixels)
            {
                sum += pixel;
            }

            return sum;
        }

        public void TryWithEmgu(Bitmap bmp)
        {
            //TODO : Try to understand EmguCV
        }

        public static long[] CreateOwnHistogram(Bitmap img)
        {
            var hist_gray = new long[256];
            var hist_red = new long[256];
            var hist_green = new long[256];
            var hist_blue = new long[256];

            for (var x = 0; x < img.Width; ++x)
            {
                for (var y = 0; y < img.Height; ++y)
                {
                    var currP = img.GetPixel(x, y);
                    hist_blue[currP.B]++;
                    hist_green[currP.G]++;
                    hist_red[currP.R]++;
                    var tempBrightness = (currP.B + currP.G + currP.R) / 3;
                    hist_gray[tempBrightness]++;
                }
            }

            return hist_gray;
        }

        private long CreateIntensitySum(long[] hist)
        {
            var sum = 0L;
            foreach (var x in hist)
            {
                sum += x;
            }

            return sum;
        }

        public double OtsuOwn(ref Bitmap img)
        {
            // create the histogram of the image
            var hist = CreateOwnHistogram(img);

            var bestThresh = 0;

            var lowerClassPixelCount = 0;
            var lowerClassIntensitySum = CreateIntensitySum(hist);
            for (var thresh = 0; thresh < hist.Length; ++thresh)
            {
                
            }

            return 0.0d;
        }

        /// <summary>Die Funktion gibt den Binarisierungsschwellenwert für ein Halbtonbild mit einer Gesamtanzahl von Pixeln zurück.</summary>
        /// <param name="image">Enthält die Intensität des Bildes von 0 bis einschließlich 255.</param>
        public double Otsu(ref Bitmap bmp)
        {
            var colors = new Color[bmp.Width, bmp.Height];
            RgbToGrayscale(ref colors, ref bmp);
            var pixels = colors2dToInt1d(ref colors);

            var histogram = new int[256];
            calculateHistogram(ref pixels, pixels.Length, ref histogram);

            // Wird benötigt, um den Unterschied in den Varianzen zwischen den Klassen schnell neu zu berechnen
            var allPixelCount = pixels.Length;
            var allIntensitySum = calculateIntensitySum(ref pixels, pixels.Length);

            var bestTresh = 0;
            var bestSigma = 0.0d;

            var firstClassPixelCount = 0;
            var firstClassIntensitySum = 0;

            // Schleife über die zwei Klassen
            // thresh < INTENSITY_LAYER_NUMBER-1, weil bei 255 ein int-overflow passiert
            for (var thresh = 0; thresh < histogram.Length; ++thresh)
            {
                firstClassPixelCount += histogram[thresh];
                firstClassIntensitySum += thresh * histogram[thresh];

                var firstClassProb = firstClassPixelCount / (double)allPixelCount;
                var secondClassProb = 1.0d - firstClassProb;

                var firstClassMean = firstClassIntensitySum / (double)firstClassPixelCount;
                var secondClassMean = (allIntensitySum - firstClassIntensitySum) /
                                      (double)(allPixelCount - firstClassPixelCount);

                var meanDelta = firstClassMean - secondClassMean;

                var sigma = firstClassProb * secondClassProb * meanDelta * meanDelta;

                if (sigma > bestSigma)
                {
                    bestSigma = sigma;
                    bestTresh = thresh;
                }
            }

            return bestTresh/255.0d;
        }

        public double ThresholdWithStackOverflow(ref Bitmap img)
        {
            var avgBright = 0.0d;
            for (var i = 0; i < img.Width; ++i)
            {
                for (var j = 0; j < img.Height; ++j)
                {
                    avgBright += img.GetPixel(i, j).GetBrightness();
                }
            }

            avgBright = avgBright / (img.Height * img.Width);
            avgBright = avgBright < .3d ? .3 : avgBright;
            return avgBright > .7d ? .7d : avgBright;
        }

        /// <summary>
        /// Applies the threshold onto a new Bitmap.
        /// </summary>
        /// <param name="img">From where the pixels are read</param>
        /// <param name="thresh">The threshold to use</param>
        /// <returns>A Bitmap with the threshold applied orignal</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Bitmap ApplyThreshold(ref Bitmap img, double thresh)
        {
            /*if ((img.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
            {
                Console.WriteLine("Is indexed, need to abort (for now)");
                throw new NotImplementedException("Cannot process indexed images yet");
            }*/

            var tempImg = new Bitmap(img.Width, img.Height);
            using (var g = Graphics.FromImage(tempImg))
            {
                for (var i = 0; i < img.Width; ++i)
                {
                    for (var j = 0; j < img.Height; ++j)
                    {
                        var p = img.GetPixel(i, j);

                        if (p.GetBrightness() >= p.GetBrightness() * thresh)
                        {
                            p = Color.White;
                        }
                        else
                        {
                            p = Color.Black;
                        }
                        
                        tempImg.SetPixel(i,j, p);
                    }
                }
            }

            return tempImg;
        }
    }
}