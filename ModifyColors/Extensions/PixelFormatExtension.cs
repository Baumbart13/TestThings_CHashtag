namespace ModifyColors.Extensions
{
    public static class PixelFormatExtension
    {
        public static int GetBitsPerPixel(this ColorFormat colorFormat)
        {
            return 8 * (int)colorFormat;
        }
    }
}