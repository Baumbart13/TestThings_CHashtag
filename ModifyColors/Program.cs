﻿using System;
using System.Collections.Generic;
using System.IO;

using ModifyColors.Extensions;

using CommandLine;

using NLog.Targets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ModifyColors
{
    class Program
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private class Options
        {
            [Option('c', "contrast", Required = false, Default = 1.0d,
                HelpText = "The change of the contrast reaching from 0,0 to 2,0 as a double")]
            public double ContrastValue { get; set; }

            [Option('b', "brightness", Required = false, Default = 0,
                HelpText = "The change of the brightness reaching from -2,0 to 2,0 as a double")]
            public double BrightnessValue { get; set; }
            
            [Option('g', "grayscale", Required = true, Default = GrayscaleMethod.AVG,
                HelpText = "What kind of grayscaling shall be used as there is not only 1 way to achieve a grayscaled image")]
            public GrayscaleMethod GrayscaleMethod { get; set; }

            [Option("threshold", Required = false, Default = ThresholdMethod.None,
                HelpText = "Choose the method you want to use to threshold the images")]
            public ThresholdMethod ThreshMethod { get; set; }

            [Option("input", Required = false, Default = "./res")]
            public string Input { get; set; }

            [Option("output", Required = false, Default = "./res/output")]
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
                logger.Info($"Loading source files");
                var src = file.Replace('\\', '/');

                
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
            logger.Info("Preparing...");
            var src = opts.Input.RemoveElement('"');
            if (!Directory.Exists(src))
            {
                logger.Error($"{src} does not exist. Please select an existing directory");
                return;
            }
            var dest = opts.Output.RemoveElement('"');
            
            var contrast = opts.ContrastValue;
            var brightness = opts.BrightnessValue;
            var grayMethod = opts.GrayscaleMethod;

            var threshMethod = opts.ThreshMethod;

            ClearDirectory(dest);

            logger.Info("Starting work");
            var files = ReadAllDirs(src, dest);
            foreach (var d in files)
            {
                if (!d.Src.Contains("TeslaCropped"))
                    continue;
                var b = -2.0m;
                while(b <= 2.0m)
                {
                    DoConversion(threshMethod, d.Src, d.Dest, contrast, double.Parse(b.ToString()), grayMethod);
                    b += 0.01m;
                    return;
                }
                break;
            }
        }

        static void Main(string[] args)
        {
            Target.Register<ModLogger>("Modify Colors");
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
        private static void DoConversion(ThresholdMethod threshMethod, string bitmapToUse, string bitmapToSave,
            double contrastValue, double brightnessValue, GrayscaleMethod grayMethod)
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
            SaveFile(CreateStringForSaving(bitmapToSave, $"{counter++}afterLoading"), img);

            if (img == null)
            {
                logger.Error($"Couldn't load file \"{bitmapToUse}\". Continuing with next file.");
                return;
            }

            var thresh = .0d;
            if (threshMethod != ThresholdMethod.None)
            {
                logger.Info("Creating the threshold");
            }
            switch (threshMethod)
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
            mp.RgbToGrayscale(grayMethod, img);
            SaveFile(CreateStringForSaving(bitmapToSave, $"{counter++}afterGrayscale"), img);
            

            if (Math.Abs(contrastValue - 1.0d) > 0.000001d)
            {
                logger.Info($"Applying a contrast of {contrastValue:F2}");
                mp.AdjustContrast(img, contrastValue);
                SaveFile(CreateStringForSaving(bitmapToSave, $"{counter++}afterContrast"), img);
            }

            if(brightnessValue != 0)
            {
                logger.Info($"Applying a brightness of {brightnessValue:F2}");
                mp.AdjustBrightness(img, brightnessValue);
                SaveFile(CreateStringForSaving(bitmapToSave, $"{counter++}afterBrightness"), img);
            }
            
            
            if (threshMethod != ThresholdMethod.None)
            {
                logger.Info($"Applying a threshold of {thresh}");
                mp.ApplyThreshold(img, thresh);
                SaveFile(CreateStringForSaving(bitmapToSave, $"{counter++}afterThreshold"), img);
            }

            var filePath = CreateStringForSaving(bitmapToSave, contrastValue, brightnessValue, grayMethod,
                threshMethod, thresh);
            
            SaveFile(filePath, img);

            logger.Info($"image No{++counter} saved");
            img.Dispose();
        }

        private static string CreateStringForSaving(string imgSave, string counter)
        {
            var first = imgSave.Substring(0, imgSave.LastIndexOf('.'));
            first += $"_{counter}_";
            first += imgSave.Substring(imgSave.LastIndexOf('.'));
            return first;
        }
        
        private static string CreateStringForSaving(string imgSave, double contrast, double brightness,
            GrayscaleMethod grayMethod, ThresholdMethod threshMethod, double thresh)
        {
            var fileEnding = imgSave.Substring(imgSave.LastIndexOf('.'));
            var path = imgSave.Substring(0, imgSave.LastIndexOf('.'));

            var completeFilePath = $"{path}";

            if (Math.Abs(contrast - 1.0d) > 0.000001d)
            {
                completeFilePath += $"-contrast_{contrast:F2}";
            }

            if (Math.Abs(brightness - 1.0d) > 0.000001d)
            {
                completeFilePath += $"-brightness_{brightness:F2}";
            }
            
            completeFilePath += $"-grayscale_{grayMethod}";
            
            if (threshMethod != ThresholdMethod.None)
            {
                completeFilePath +=
                    $"-threshVal_{thresh:F2}" +
                    $"-threshMethod_{threshMethod}";
            }
            completeFilePath += $"{fileEnding}";

            return completeFilePath;
        }

        private static void ClearDirectory(string path)
        {
            if(Directory.Exists(path))
            {
                logger.Info($"This path will be deleted:\n\"{path}\"\n");
                Directory.Delete(path, true);
            }
        }

        private static void SaveFile(string filePath, Image<Rgba32> img)
        {
            var dirPath = new DirectoryInfo(filePath).Parent.FullName;
            logger.Info($"Directory to be created: \"{dirPath}\"");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            
            logger.Info($"Saving image to \"{filePath}\"");
            using (var outStream = File.Create(filePath))
            {
                img.Save(outStream, new PngEncoder());
            }
            img.Save(filePath);
        }
    }
}