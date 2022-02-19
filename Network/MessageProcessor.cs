using System.Text;
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
            
            var width = msg.Content[0].Value.Integer;
            var height = msg.Content[1].Value.Integer;
            /*var format = (PixelFormat)msg.Content[2].Value.Integer;
            var bmp = new Bitmap(width, height, format);
            
            return bmp;*/
            return new Image<Rgba32>(1,1); // TODO: Need to refactor!!!
        }
    }
}