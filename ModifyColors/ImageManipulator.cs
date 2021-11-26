using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Filters;

namespace ModifyColors
{
    public class ImageManipulator
    {
        private string lastMethodCalled = "";
        
        public const int INTENSITY_LAYER_NUMBER = 256;

        public Bitmap RgbToGrayscale(GrayscaleMethod grayMethod, Image img)
        {
            var pixels = img.Clone<>();

            if (grayMethod == GrayscaleMethod.BT601)
            {
                pixels.Mutate(i=>
                    i.Grayscale(GrayscaleMode.Bt601));
                return pixels;
            }

            for (var i = 0; i < pixels.GetLength(0); ++i)
            {
                for (var j = 0; j < pixels.GetLength(1); ++j)
                {
                    img.Mutate(i =>
                    {
                        i.
                    });
                    var p = img.GetPixel(i, j);
                    int gray = 0;

                    var r = p.R;
                    var g = p.G;
                    var b = p.B;
                    switch(grayMethod){
                        case GrayscaleMethod.CV_RGB2GRAY:
                            gray = (int)(0.30 * r + 0.59 * g + 0.11 * b);
                            break;
                        case GrayscaleMethod.SRGB:
                            var linear = (int) (0.2126 * r + 0.7152 * g + 0.0722 * b);
                            gray = linear <= 0.0031308
                                ? (int) (12.92 * linear)
                                : (int) (1.055 * Math.Pow(linear, 0.0416666666667d) - 0.055);
                            break;
                        case GrayscaleMethod.LINEAR:
                            gray = (int) (0.2126 * r + 0.7152 * g + 0.0722 * b);
                            break;
                        case GrayscaleMethod.REC601:
                            gray = (int) (0.299 * r + 0.587 * g + 0.114 * b);
                            break;
                        case GrayscaleMethod.BT709:
                            gray = (int) (0.2126 * r + 0.7152 * g + 0.0722 * b);
                            break;
                        case GrayscaleMethod.BT2100:
                            gray = (int) (0.2627 * r + 0.6780 * g + 0.0593 * b);
                            break;
                        case GrayscaleMethod.AVG:
                        default:
                            gray = (r+g+b) / 3;
                            break;
                    }

                    var grayByte = (byte) (gray % 256);
                    pixels[i, j] = Color.FromRgb(grayByte, grayByte, grayByte);
                }
            }

            return pixels;
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

        private int calculateIntensitySum(ref int[] pixels, int pixelCount)
        {
            var sum = 0;
            foreach (var pixel in pixels)
            {
                sum += pixel;
            }

            return sum;
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
                    var tempGray = (currP.B + currP.G + currP.R) / 3;
                    hist_gray[tempGray]++;
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

        internal struct XY
        {
            public int X;
            public int Y;
        }
        
        public double AutomaticOwn(Bitmap img)
        {
            this.lastMethodCalled = "AutomaticOwn";
            
            var bestThresh = 10;
            const int SOME_LIMIT = 5;

            while(true)
            {

                var lower = new List<XY>();
                var upper = new List<XY>();
                for (var x = 0; x < img.Width; ++x)
                {
                    for (var y = 0; y < img.Height; ++y)
                    {
                        if (img.GetPixel(x, y).GetBrightness() > (bestThresh / 255d))
                        {
                            upper.Add(new XY { X = x, Y = y });
                        }
                        else
                        {
                            lower.Add(new XY { X = x, Y = y });
                        }
                    }
                }

                var lowerMean = 0;
                foreach (var p in lower)
                {
                    lowerMean += img.GetPixel(p.X, p.Y).B;
                }

                lowerMean = lowerMean / lower.Count;

                var upperMean = 0;
                foreach (var p in upper)
                {
                    upperMean += img.GetPixel(p.X, p.Y).B;
                }

                upperMean = upperMean / upper.Count;

                var thresh = (upperMean + lowerMean) / 2;

                if (Math.Abs(bestThresh - thresh) < SOME_LIMIT)
                {
                    bestThresh = thresh;
                    break;
                }

                bestThresh = thresh;

                lower.Clear();
                upper.Clear();
            }

            return bestThresh > 1.0d ? bestThresh / 256.0d : bestThresh;
        }

        /// <summary>Die Funktion gibt den Binarisierungsschwellenwert für ein Halbtonbild mit einer Gesamtanzahl von Pixeln zurück.</summary>
        /// <param name="image">Enthält die Intensität des Bildes von 0 bis einschließlich 255.</param>
        public double Otsu(Bitmap bmp)
        {
            this.lastMethodCalled = "OtsuWikipedia";
            
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

        public double ThresholdWithStackOverflow(Bitmap img)
        {
            this.lastMethodCalled = "ThreshWithStackOverflow";
            
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
        
        public double ThresholdWithStackOverflowOther(Bitmap img)
        {
            this.lastMethodCalled = "ThreshWithStackOverflowOther";
            
            var avgBright = 0.0d;
            for (var i = 0; i < img.Width; ++i)
            {
                for (var j = 0; j < img.Height; ++j)
                {
                    avgBright += img.GetPixel(i, j).GetBrightness();
                }
            }

            return avgBright = avgBright / (img.Height * img.Width);
            avgBright = avgBright < .3d ? .3 : avgBright;
            return avgBright > .7d ? .7d : avgBright;
        }

        /// <summary>
        /// Applies the threshold onto a new Bitmap.
        /// </summary>
        /// <param name="img">From where the pixels are read</param>
        /// <param name="thresh">The threshold to use</param>
        /// <param name="fast">Safe and Slow or Unsafe and Fast</param>
        /// <returns>A Bitmap with the threshold applied orignal</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Bitmap ApplyThreshold(Bitmap img, double thresh, bool fast = false)
        {
            /*if ((img.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
            {
                Console.WriteLine("Is indexed, need to abort (for now)");
                throw new NotImplementedException("Cannot process indexed images yet");
            }*/
            
            var tempImg = new Bitmap(img.Width, img.Height);

            if (fast)
            {
                throw new NotImplementedException();
            }
            else
            {
                for (var i = 0; i < img.Width; ++i)
                {
                    for (var j = 0; j < img.Height; ++j)
                    {
                        var p = img.GetPixel(i, j);
                        var gray = (p.B + p.G + p.R) / (3 * 256d);

                        if (p.GetBrightness() >= thresh)
                            //if (gray >= thresh)
                        {
                            p = Color.White;
                        }
                        else
                        {
                            p = Color.Black;
                        }

                        tempImg.SetPixel(i, j, p);
                    }
                }
            }

            img.Dispose();

            return tempImg;
        }

        public override string ToString()
        {
            return lastMethodCalled;
        }
    }
}