using System;
using System.Drawing;
using System.IO;
using System.IO.Enumeration;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;


namespace ModifyColors
{
    class Program
    {
        public const string imageSrc_wikipedia =
            @"C:\Users\Baumbart13\RiderProjects\TestThings\ModifyColors\res\Image_processing_pre_otsus_algorithm.jpg";

        public const string imageSrc_own = @"C:\Users\Baumbart13\RiderProjects\TestThings\ModifyColors\res\Unbenannt.png";

        public const string imageDest_wikipedia = @"C:\Users\Baumbart13\Desktop\Unnamed_wiki.png";
        public const string imageDest_own = @"C:\Users\Baumbart13\Desktop\Unnamed_own.png";

        static void Main(string[] args)
        {
            var mp = new ImageManipulator();

            Bitmap img = new Bitmap(imageSrc_wikipedia);
            
            var otsu = mp.Otsu(ref img);
            var avgBright = mp.ThresholdWithStackOverflow(ref img);
            var otsuOwn = mp.OtsuOwn(ref img);
            
            img = mp.ApplyThreshold(ref img, otsu);
            img.Save(imageDest_wikipedia);
            Console.WriteLine("First image saved");

            // another image
            img = new Bitmap(imageSrc_own);
            
            otsu = mp.Otsu(ref img);
            avgBright = mp.ThresholdWithStackOverflow(ref img);
            otsuOwn = mp.OtsuOwn(ref img);
            
            img = mp.ApplyThreshold(ref img, otsu);
            img.Save(imageDest_own);
            Console.WriteLine("Second image saved");
        }
    }
}