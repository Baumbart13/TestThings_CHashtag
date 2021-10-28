using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Enumeration;
using System.Reflection.PortableExecutable;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;


namespace ModifyColors
{
    class Program
    {
        public const string imageSrc = @"C:\Users\Baumbart13\RiderProjects\TestThings\ModifyColors\res";
        public readonly static string imageDest = @$"{imageSrc}\output";

        public struct Dirs
        {
            public string Src;
            public string Dest;
        }

        private static IEnumerable<Dirs> ReadAllDirs()
        {
            foreach (var file in Directory.GetFiles(imageSrc))
            {
                var src = file;

                var fileName = file.Substring(file.LastIndexOf('\\'));
                var destDir = $"{imageDest}{fileName}";

                yield return new Dirs { Dest = destDir, Src = src };
            }
        }

        static void Main(string[] args)
        {
            ClearDirectory(imageDest);

            var files = ReadAllDirs();
            foreach(var d in files)
            {
                DoConversion(mp.Otsu, d.Src, d.Dest);
                DoConversion(mp.AutomaticOwn, d.Src, d.Dest);
                DoConversion(mp.ThresholdWithStackOverflow, d.Src, d.Dest);
            }
        }

        private static ImageManipulator mp = new ImageManipulator();
        private static int counter = 0;
        private static void DoConversion(Func<Bitmap, double> methodToUse, string bitmapToUse, string bitmapToSave)
        {
            var img = new Bitmap(bitmapToUse);
            var thresh = methodToUse(img);
            //var thresh = 100 / 256d;
            img = mp.ApplyThreshold(img, thresh);
            
            var fileEnding = bitmapToSave.Substring(bitmapToSave.LastIndexOf('.'));
            var path = bitmapToSave.Substring(0, bitmapToSave.LastIndexOf('.'));
            var completeFilePath = $"{path}_{(int)(256d * thresh)}_{mp}{fileEnding}";

            var dirPath = new DirectoryInfo(path).Parent.FullName;

            Console.WriteLine($"Directory to be created: \"{dirPath}\"");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            Console.WriteLine($"File's complete path: \"{completeFilePath}\"");
            img.Save(completeFilePath);
            Console.WriteLine($"Image No{++counter} saved");
            
            img.Dispose();
        }

        private static void ClearDirectory(string path)
        {
            if(Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}