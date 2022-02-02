using System;
using System.Drawing;
using System.Drawing.Imaging;
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
            if (msg.FrameCount != 3)
            {
                throw new InvalidDataException("There are not 3 frames, as it should be");
            }
            var containerFrame = ReadContainerFrame(msg[0]);
            var messageFrame = ReadMessageFrame(msg[1]);
            var contentFrame = ReadContentFrame(msg[2]);
            

            return null;
        }

        private static MessageType ReadContainerFrame(NetMQFrame frame)
        {
            var b = frame.ToByteArray();
            var messageTypeI = (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
            return (MessageType) messageTypeI;
        }

        private static List<MessageArgument> ReadMessageFrame(NetMQFrame frame)
        {
            var l = new List<MessageArgument>();
            // TODO: Implement Network.MessageProcessor.ReadMessageFrame(NetMQFrame) based on Network.Message.CreateMessageFrame()
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

        public static Image<Rgba32> ReadImage(this Message msg)
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