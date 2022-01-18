namespace ModifyColors.Extensions
{
    public static class PixelFormatExtension
    {
        public static int GetBitsPerPixel(this PixelFormat pixelFormat)
        {
            return 8 * (int)pixelFormat;
        }
    }
}