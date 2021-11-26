using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Enumeration;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

using CommandLine;

using NLog;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ModifyColors
{
    class Program
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private class Options
        {
            [Option('c', "contrast", Required = true, Default = 1.0d,
                HelpText = "The change of the contrast reaching from 0,0 to 2,0 as a double")]
            public double ContrastValue { get; set; }

            [Option('b', "brightness", Required = true, Default = 0,
                HelpText = "The change of the brightness reaching from 0 to 256 as an integer")]
            public int BrightnessValue { get; set; }
            
            [Option('g', "grayscale", Required = true, Default = GrayscaleMethod.AVG,
                HelpText = "What kind of grayscaling shall be used as there is not only 1 way to achieve a grayscaled image")]
            public GrayscaleMethod GrayscaleMethod { get; set; }

            [Option("threshold", Required = false, Default = ThresholdMethod.None,
                HelpText = "Choose the method you want to use to threshold the images")]
            public ThresholdMethod ThreshMethod { get; set; }

            [Option("input", Required = false, Default = "/home/baumbart13/RiderProjects/TestThings/ModifyColors/res")]
            public string Input { get; set; }

            [Option("output", Required = false, Default = "/home/baumbart13/RiderProjects/TestThings/ModifyColors/res/output")]
            public string Output { get; set; }

            [Option("debug", Required = false, Default = false, Hidden = true)]
            public bool DebugMode { get; set; }
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
                
                logger.Info(
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
            var grayMethod = opts.GrayscaleMethod;

            var threshMethod = opts.ThreshMethod;

            ClearDirectory(dest);

            var files = ReadAllDirs(src, dest);
            foreach (var d in files)
            {
                DoConversion(threshMethod, src, dest, contrast, brightness, grayMethod);
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
            double contrastValue, int brightnessValue, GrayscaleMethod grayMethod)
        {
            if (!DoesFileWork(bitmapToUse))
            {
                logger.Warn($"Skipping file \"{bitmapToUse}\". Only png, jpg/.jpeg compatible for now");
                return;
            }

            if (!File.Exists(bitmapToUse))
            {
                logger.Warn($"Skipping file \"{bitmapToUse}\", due to the fact, that this file does not exist");
            }

            Image<Rgba32> img = null;
            using(var inStream = File.Open(bitmapToUse, FileMode.Open))
            {
                img = Image.Load(inStream).CloneAs<Rgba32>();
            }

            if (img == null)
            {
                logger.Error($"Couldn't load file \"{bitmapToUse}\". Continuing with next file.");
                return;
            }

            var thresh = .0d;
            if (methodToUse != ThresholdMethod.None)
            {
                logger.Info("Creating the threshold");
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
                    logger.Warn("Attention, this method is not recommended!");
                    thresh = mp.ThresholdWithStackOverflowOther(img);
                    break;
            }

            logger.Info("Converting to grayscale");
            img = mp.RgbToGrayscale(grayMethod, img);
            
            logger.Info($"Applying a contrast of {contrastValue}");
            img = mp.AdjustContrast(img, contrastValue);
            
            logger.Info($"Applying a brightness of {brightnessValue}");
            img = mp.AdjustBrightness(img, brightnessValue);
            
            
            if (methodToUse != ThresholdMethod.None)
            {
                logger.Info($"Applying a threshold of {thresh}");
                img = mp.ApplyThreshold(img, thresh);
            }

            var fileEnding = bitmapToSave.Substring(bitmapToSave.LastIndexOf('.'));
            var path = bitmapToSave.Substring(0, bitmapToSave.LastIndexOf('.'));

            var completeFilePath =
                $"{path}" +
                $"-contrast_{Math.Round(contrastValue, 6)}" +
                $"-brightness_{brightnessValue}" +
                $"-grayscale_{grayMethod}";
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
            
            logger.Info($"Directory to be created: \"{dirPath}\"");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            
            logger.Info($"Saving image to \"{completeFilePath}\"");
            using (var outStream = File.Create(completeFilePath))
            {
                img.Save(outStream, new PngEncoder());
            }
            img.Save(completeFilePath);
            
            logger.Info($"image No{++counter} saved");
            img.Dispose();
        }

        private static void ClearDirectory(string path)
        {
            logger.Info($"This path will be deleted:\n\"{path}\"\n");
            if(Directory.Exists(path))
            {
                if(logger.IsDebugEnabled)
                    logger.Debug("Exiting, because folder wants to get deleted");
                Environment.Exit(0);
                //Directory.Delete(path, true);
            }
        }
    }
}