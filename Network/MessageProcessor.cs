using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace Network
{
    public static class MessageParseExtension
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static object Process(this Message msg)
        {
            switch (msg.MessageType)
            {
                case MessageType.Image:
                    return ReadImage(msg);
                case MessageType.String:
                    return ReadString(msg);
                default:
                    Logger.Error($"This message with type {(int)msg.MessageType} does not exist. Length:{msg.Content.Count}");
                    break;
            }

            return null;
        }

        public static object ReadString(this Message msg)
        {
            var length = msg.Content[0].Value.Integer;
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; ++i)
            {
                sb.Append(msg.Content[i + 1].Value.Character);
            }

            return sb.ToString();
        }

        public static Bitmap ReadImage(this Message msg)
        {
            if (msg.MessageType != MessageType.Image)
            {
                throw new InvalidOperationException("An Image can only be read from a Message that contains a Message");
            }
            
            var width = msg.Content[0].Value.Integer;
            var height = msg.Content[1].Value.Integer;
            var format = (PixelFormat)msg.Content[2].Value.Integer;
            var bmp = new Bitmap(width, height, format);
            
            return bmp;
        }
    }
}