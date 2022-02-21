using System.Text;
using ModifyColors;
using NetMQ;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Network
{
    public static class MessageParseExtension
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static Message ToArtemisMessage(this NetMQMessage msg)
        {
            //var artemisMessage = new Message();
            var containerFrame = msg[0];
            //containerFrame.

            return null;
        }

        public static string ReadString(this Message msg)
        {
            var length = msg.Content[0].Value.Integer;
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; ++i)
            {
                sb.Append(msg.Content[i + 1].Value.Character);
            }

            return sb.ToString();
        }

        public static Image<Rgba32> ReadImage(this Message msg)
        {
            if (msg.MessageType != MessageType.Image)
            {
                throw new InvalidOperationException("An Image can only be read from a Message that contains an Image");
            }

            var i = 0;
            var width = msg.Content[i++].Value.Integer;
            var height = msg.Content[i++].Value.Integer;
            var format = (ColorFormat)msg.Content[i++].Value.Integer;
            var img = new Image<Rgba32>(width, height);
            
            // fill pixels
            switch (format)
            {
#region Rgba32
                case ColorFormat.Rgba32:
                    for (var x = 0; x < width; ++x)
                    {
                        for (var y = 0; y < height; ++y)
                        {
                            var r = msg.Content[i++].Value.Byte;
                            var g = msg.Content[i++].Value.Byte;
                            var b = msg.Content[i++].Value.Byte;
                            img[x, y] = new Rgba32(r,g,b);
                        }
                    }
                    break;
#endregion
#region Grayscale
                case ColorFormat.Grayscale:
                    for (var x = 0; x < width; ++x)
                    {
                        for (var y = 0; y < height; ++y)
                        {
                            var gray = msg.Content[i++].Value.Byte;
                            img[x, y] = new Rgba32(gray, gray, gray);
                        }
                    }
                    break;
#endregion
                default:
                    throw new NotSupportedException("Only RGB and Greyscale images are supported");
            }
            ImageManipulator.CheckAndCorrectColorFormat(img);
            return img;
        }
    }
}