using System;
using System.Collections.Generic;
using System.Text;
using ModifyColors.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ModifyColors
{
    public class ImageManipulator
    {
        private string lastMethodCalled = "";

        public const int INTENSITY_LAYER_NUMBER = 256;

        public void RgbToGrayscale(GrayscaleMethod grayMethod, Image<Rgba32> img)
        {

            if (grayMethod == GrayscaleMethod.BT601)
            {
                img.Mutate(i =>
                    i.Grayscale(GrayscaleMode.Bt601));
                return;
            }

            for (var i = 0; i < img.Width; ++i)
            {
                for (var j = 0; j < img.Height; ++j)
                {
                    var p = img[i, j];
                    int gray = 0;

                    var r = p.R;
                    var g = p.G;
                    var b = p.B;
                    switch (grayMethod)
                    {
                        case GrayscaleMethod.CV_RGB2GRAY:
                            gray = (int) (0.30 * r + 0.59 * g + 0.11 * b);
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
                            gray = (r + g + b) / 3;
                            break;
                    }

                    var grayByte = (byte) (gray % 256);
                    img[i, j] = new Rgba32(grayByte, grayByte, grayByte);
                }
            }

            return;
        }

        public void AdjustContrast(Image<Rgba32> img, double change)
        {
            var contrast = ((100.0 + change) * 0.01d) * ((100.0 + change) * 0.01);
            
            change = change - 1.0d;
            change = (100.0d + change) * 0.01d;
            change = change * change;

            // TODO: Fix contrast adjustment

            for (int i = 0; i < img.Width; ++i)
            {
                for (int j = 0; j < img.Height; ++j)
                {
                    var old = img[i,j];
                    var red = ((((old.R / 255.0) -0.5) * contrast) +0.5) * 255.0;
                    var green = ((((old.G / 255.0) -0.5) * contrast) +0.5) * 255.0;
                    var blue = ((((old.B / 255.0) -0.5) * contrast) +0.5) * 255.0;

                    var iR = (byte)((int)red);
                    var iG = (byte)((int)green);
                    var iB = (byte)((int)blue);

                    img[i, j] = new Rgba32(iR, iG, iB);
                }
            }

            return;
        }

        /// <summary>
        /// Changes the brightness of the picture
        /// </summary>
        /// <param name="img">The picture source</param>
        /// <param name="change">A signed byte reaching from -127 to 127</param>
        /// <returns>The adjusted image</returns>
        public void AdjustBrightness(Image<Rgba32> img, sbyte change)
        {
            var changeCorrected = 0.00590551181d * (double)change + 1.25d;
            
            for (var i = 0; i < img.Width; ++i)
            {
                for(var j = 0; j < img.Height; ++j)
                {
                    int r = img[i, j].R;
                    int g = img[i, j].G;
                    int b = img[i, j].B;
                    if (b > 240 || b < 10)
                    {
                        Console.WriteLine($"RGB value in image is {b}");
                        Console.ReadKey();
                    }
                    
                    r = (int)Math.Ceiling(r * changeCorrected);
                    g = (int)Math.Ceiling(g * changeCorrected);
                    b = (int)Math.Ceiling(b * changeCorrected);
                    if (b > 255 || b < 0)
                    {
                        Console.WriteLine($"Changed RGB value is {b}");
                        Console.ReadKey();
                    }
                    
                    // clamp rgb between 0 and 255
                    r = r < 0 ? 0 : (r > 0xFF ? 0xFF : r);
                    g = g < 0 ? 0 : (g > 0xFF ? 0xFF : g);
                    b = b < 0 ? 0 : (b > 0xFF ? 0xFF : b);

                    if (b > 255 || b < 0)
                    {
                        Console.WriteLine($"Clamped value should not be this, but truly it is {b}");
                        Console.ReadKey();
                    }
                    img[i, j] = new Rgba32(r, g, b);
                }
            }

            return;
        }

        private int[] colors2dToInt1d(Rgba32[,] colors)
        {
            var pixels = new int[colors.Length];
            var counter = 0;
            foreach (var i in colors)
            {
                pixels[counter++] = i.B;
            }

            return pixels;
        }

        private void CalculateHistogram(Image<Rgba32> img, int[] hist, int[] histR, int[] histG, int[] histB)
        {
            Array.Clear(hist, 0, hist.Length);
            Array.Clear(histR, 0, histR.Length);
            Array.Clear(histG, 0, histG.Length);
            Array.Clear(histB, 0, histB.Length);

            for (var i = 0; i < img.Width; ++i)
            {
                for (var j = 0; j < img.Height; ++j)
                {
                    var p = img[i, j];
                    ++hist[p.Rgba];
                    ++histR[p.R];
                    ++histG[p.G];
                    ++histB[p.B];
                }
            }
        }

        /// <summary>
        /// Calculates the intensity-sum
        /// </summary>
        /// <param name="img">The image from where the sum gets created</param>
        /// <returns>The intensity-sum</returns>
        /// <exception cref="NotSupportedException">gets thrown in case of not parsing an grayscale-image</exception>
        private int CalculateIntensitySum(Image<Rgba32> img)
        {
            var sum = 0;

            for (var i = 0; i < img.Width; ++i)
            {
                for (var j = 0; j < img.Height; ++j)
                {
                    var p = img[i, j];
                    if (p.R == p.G || p.R == p.B || p.G == p.B)
                    {
                        throw new NotSupportedException("Only grayscale-images allowed!");
                    }

                    sum += img[i, j].B;
                }
            }

            return sum;
        }


        internal struct XY
        {
            public int X;
            public int Y;
        }

        public double AutomaticOwn(Image<Rgba32> img)
        {
            this.lastMethodCalled = "AutomaticOwn";

            var bestThresh = 10;
            const int SOME_LIMIT = 5;

            while (true)
            {
                var lower = new List<XY>();
                var upper = new List<XY>();
                for (var x = 0; x < img.Width; ++x)
                {
                    for (var y = 0; y < img.Height; ++y)
                    {
                        if (img[x, y].GetBrightness() > (bestThresh / 255d))
                        {
                            upper.Add(new XY {X = x, Y = y});
                        }
                        else
                        {
                            lower.Add(new XY {X = x, Y = y});
                        }
                    }
                }

                var lowerMean = 0;
                foreach (var p in lower)
                {
                    lowerMean += img[p.X, p.Y].B;
                }

                lowerMean = lowerMean / lower.Count;

                var upperMean = 0;
                foreach (var p in upper)
                {
                    upperMean += img[p.X, p.Y].B;
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
        public double Otsu(Image<Rgba32> img)
        {
            this.lastMethodCalled = "OtsuWikipedia";

            var colors = new Rgba32[img.Width, img.Height];
            var pixels = colors2dToInt1d(colors);

            var histogram = new int[256];
            CalculateHistogram(img, histogram, new int[1], new int[1], new int[1]);

            // Wird benötigt, um den Unterschied in den Varianzen zwischen den Klassen schnell neu zu berechnen
            var allPixelCount = pixels.Length;
            var allIntensitySum = CalculateIntensitySum(img);

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

                var firstClassProb = firstClassPixelCount / (double) allPixelCount;
                var secondClassProb = 1.0d - firstClassProb;

                var firstClassMean = firstClassIntensitySum / (double) firstClassPixelCount;
                var secondClassMean = (allIntensitySum - firstClassIntensitySum) /
                                      (double) (allPixelCount - firstClassPixelCount);

                var meanDelta = firstClassMean - secondClassMean;

                var sigma = firstClassProb * secondClassProb * meanDelta * meanDelta;

                if (sigma > bestSigma)
                {
                    bestSigma = sigma;
                    bestTresh = thresh;
                }
            }

            return bestTresh / 255.0d;
        }

        public double ThresholdWithStackOverflow(Image<Rgba32> img)
        {
            this.lastMethodCalled = "ThreshWithStackOverflow";

            var avgBright = 0.0d;
            for (var i = 0; i < img.Width; ++i)
            {
                for (var j = 0; j < img.Height; ++j)
                {
                    avgBright += img[i, j].GetBrightness();
                }
            }

            avgBright = avgBright / (img.Height * img.Width);
            avgBright = avgBright < .3d ? .3 : avgBright;
            return avgBright > .7d ? .7d : avgBright;
        }

        public double ThresholdWithStackOverflowOther(Image<Rgba32> img)
        {
            this.lastMethodCalled = "ThreshWithStackOverflowOther";

            var avgBright = 0.0d;
            for (var i = 0; i < img.Width; ++i)
            {
                for (var j = 0; j < img.Height; ++j)
                {
                    avgBright += img[i, j].GetBrightness();
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
        /// <returns>A Bitmap with the threshold applied orignal</returns>
        /// <exception cref="NotImplementedException"></exception>
        public void ApplyThreshold(Image<Rgba32> img, double thresh)
        {
            /*if ((img.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
            {
                Console.WriteLine("Is indexed, need to abort (for now)");
                throw new NotImplementedException("Cannot process indexed images yet");
            }*/

            for (var i = 0; i < img.Width; ++i)
            {
                for (var j = 0; j < img.Height; ++j)
                {
                    var p = img[i, j];
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

                    img[i, j] = p;
                }
            }
        }

        public override string ToString()
        {
            return lastMethodCalled;
        }
    }
}