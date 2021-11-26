using SixLabors.ImageSharp.PixelFormats;

namespace ModifyColors.Extensions
{
    public static class Rgba32Extension
    {
        public static float GetBrightness(this Rgba32 rgb)
        {
            return (rgb.R + rgb.G + rgb.B) * .333333f;
        }
    }
}