using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Enumeration;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace ModifyColors
{
    class Program
    {

        private class Options
        {
            [Option('c', "contrast", Required=true, Default = 1.0d,
                HelpText = "The change of the cntrast reaching from 0,0 to 2,0 as a double")]
            public double ContrastValue { get; set; }

            [Option('b', "brightness", Required = true, Default = 0,
                HelpText = "The change of the brightness reaching from 0 to 256 as an integer")]
            public int BrightnessValue { get; set; }
            
            [Option("input", Required = false, Default = "/home/baumbart13/RiderProjects/TestThings/ModifyColors/res")]
            public string Input { get; set; }
            
            [Option("output", Required = false, Default = "/home/baumbart13/RiderProjects/TestThings/ModifyColors/res/output")]
            public string Output { get; set; }
            
            [Option("threshold", Required = false)]
            public ThresholdMethod ThreshMethod { get; set; }
        }

        public struct Dirs
        {
            public string Src;
            public string Dest;
        }

        private static IEnumerable<Dirs> ReadAllDirs(string imageSrc, string imageDest)
        {
            foreach (var file in Directory.GetFiles(imageSrc))
            {
                var src = file;
                src.Replace('\\', '/');

                var fileName = src.Substring(src.LastIndexOf('/'));
                var destDir = $"{imageDest}{fileName}";
                
                Console.WriteLine(
                    $"Src: \"{src}\"\n" +
                    $"Dest: \"{destDir}\"");

                yield return new Dirs { Dest = destDir, Src = src };
            }
        }

        static void Run(Options opts)
        {
            var src = opts.Input.Remove('"');
            var dest = opts.Output.Remove('"');

            var contrast = opts.ContrastValue;
            var brightness = opts.BrightnessValue;

            var threshMethod = opts.ThreshMethod;

            ClearDirectory(dest);

            var files = ReadAllDirs(src, dest);
            foreach (var d in files)
            {
                DoConversion(threshMethod, src, dest, contrast, brightness);
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => Run(opts));
        }

        static bool DoesFileWork(string fileName)
        {
            foreach (var ending in Enum.GetNames(typeof(WorkingImageFormats)))
            {
                if (fileName.EndsWith($".{ending}"))
                {
                    return true;
                }
            }

            return false;
        }

        private static ImageManipulator mp = new ImageManipulator();
        private static int counter = 0;
        private static void DoConversion(ThresholdMethod methodToUse, string bitmapToUse, string bitmapToSave,
            double contrastValue, double brightnessValue)
        {
            if (!DoesFileWork(bitmapToUse))
            {
                Console.WriteLine($"Skipping file \"{bitmapToUse}\". Only png, jpg/.jpeg compatible for now");
                return;
            }

            Image img;
            using(var inStream = new MemoryStream()){
                using(var outStream = new MemoryStream())
                {
                    IImageFormat format;
                    using (var image = Image.Load(inStream))
                    {

                    }
                }
            }

            double thresh = .0d;
            if (methodToUse != ThresholdMethod.None)
            {
                Console.WriteLine("Creating the threshold");
            }
            switch (methodToUse)
            {
                case ThresholdMethod.None:
                    break;
                case ThresholdMethod.Automatic:
                    thresh = mp.AutomaticOwn(img);
                    break;
                case ThresholdMethod.Otsu:
                    thresh = mp.Otsu(img);
                    break;
                case ThresholdMethod.StackOverflow:
                    thresh = mp.ThresholdWithStackOverflow(img);
                    break;
                case ThresholdMethod.StackOverflow2:
                    Console.WriteLine("Attention, this method is not recommended!");
                    thresh = mp.ThresholdWithStackOverflowOther(img);
                    break;
            }

            Console.WriteLine("Converting to grayscale");
            img = mp.RgbToGrayscale(img);
            Console.WriteLine($"Applying a contrast of {contrastValue}");
            img = mp.AdjustContrast(img, contrastValue);
            Console.WriteLine($"Applying a brightness of {brightnessValue}");
            img = mp.AdjustBrightness(img, brightnessValue);

            if (methodToUse != ThresholdMethod.None)
            {
                Console.WriteLine($"Applying a threshold of {thresh}");
                img = mp.ApplyThreshold(img, thresh);
            }

            var fileEnding = bitmapToSave.Substring(bitmapToSave.LastIndexOf('.'));
            var path = bitmapToSave.Substring(0, bitmapToSave.LastIndexOf('.'));

            var completeFilePath =
                $"{path}" +
                $"-contrast_{Math.Round(contrastValue, 6)}" +
                $"-brightness_{brightnessValue}";
            if (methodToUse != ThresholdMethod.None)
            {
                completeFilePath +=
                    $"-threshVal_{thresh}" +
                    $"-threshMethod_{methodToUse}" +
                    $"{fileEnding}";
            }
            else
            {
                completeFilePath += $"{fileEnding}";
            }

            var dirPath = new DirectoryInfo(path).Parent.FullName;
            
            Console.WriteLine($"Directory to be created \"{dirPath}\"");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            
            Console.WriteLine($"File's complete path: \"{completeFilePath}\"");
            img.Save(completeFilePath);
            
            Console.WriteLine($"image No{++counter} saved");
            img.Dispose();
        }

        private static void ClearDirectory(string path)
        {
            Console.WriteLine($"This path will be deleted:\n\"{path}\"\n");
            if(Directory.Exists(path))
            {
                //Directory.Delete(path, true);
            }
        }
    }
}