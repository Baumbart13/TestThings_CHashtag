using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using ModifyColors;
using ModifyColors.Extensions;
using NetMQ;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Network
{
    public class Message
    {
        private readonly List<MessageArgument> mMessageContent;
        public IReadOnlyList<MessageArgument> Content => this.mMessageContent;

        private uint mSize;
        
        /// <summary>
        /// The Size of this message in bytes
        /// </summary>
        public uint Size => mSize;
        public MessageType MessageType { init; get; }

        public Message(MessageType type)
        {
            this.MessageType = type;
            this.mMessageContent = new List<MessageArgument>();
        }

        public static Message FromNetMqMessage(NetMQMessage netMqMsg, bool isStringMsg = false)
        {
            if (isStringMsg)
            {
                return NetMqMsgToStringMsg(netMqMsg);
            }
            
            var msg = new Message(MessageType.Image);
            

            return msg;
        }

        public NetMQ.NetMQMessage ToNetMqMessage()
        {
            var netmqMsg = new NetMQMessage();

            // 4 bytes to hold a 32-bit int aka enum
            //var container = new NetMQFrame(sizeof(MessageType));
            var container = new NetMQFrame(4);
            
            // Amount of bytes needed for metadata of the message-content
            var msgMeta = new NetMQFrame(this.MessageType switch
            {
                MessageType.Image => 4 * 3, // ColorFormat, Height, Width
                MessageType.ImageRequest => 0,
                MessageType.String => 4, // string-length
                _ => throw new NotImplementedException("Other types are not implemented by now")
            });
            
            // Amount of bytes needed for the actual content of message
            var msg = new NetMQFrame(this.MessageType switch
            {
                MessageType.Image => this.mMessageContent.Count,
                MessageType.ImageRequest => 0,
                MessageType.String => this.mMessageContent.Count * 2, // a character needs 2 bytes in .NET
                _ => throw new NotImplementedException("Other types are not implemented by now")
            });

            FillMsgContainer(container);
            FillMsgContentMeta(msgMeta);
            FillMsgContent(msg);

            netmqMsg.Append(msg);
            netmqMsg.Append(msgMeta);
            netmqMsg.Append(container);

            return netmqMsg;
        }

        /// <summary>
        /// Adds the MessageType to the first NetMQFrame
        /// </summary>
        /// <param name="container">Where the MessageType shall be written to</param>
        private void FillMsgContainer(NetMQFrame container)
        {
            // set the type of message
            var iMsgType = (int)this.MessageType;
            var bArrMsgType = new byte[]
            {
                (byte)(iMsgType >> 24),
                (byte)(iMsgType >> 16),
                (byte)(iMsgType >> 8),
                (byte)(iMsgType)
            };
            
            bArrMsgType.CopyTo(container.Buffer, 0);
        }

        /// <summary>
        /// Adds the metadata of the content, like size of the packet, width and height of the image inside of the packet.
        /// </summary>
        /// <param name="frame"></param>
        private void FillMsgContentMeta(NetMQFrame frame)
        {
            // TODO: Implement Network.Message.AddMsgContentMeta(NetMQFrame)
            switch (this.MessageType)
            {
                case MessageType.Image:
                    FillMsgContentMeta_Image(frame);
                    break;
                case MessageType.String:
                    FillMsgContentMeta_String(frame);
                    break;
                default:
                    throw new NotImplementedException("Other types are not implemented by now");
                    break;
            }
        }

        private void FillMsgContentMeta_Image(NetMQFrame frame)
        {
            var iColFormat = this.mMessageContent[2].Value.Integer;
            var bArrColFormat = new byte
        }

        private void FillMsgContentMeta_String(NetMQFrame frame)
        {
            // TODO: Implement Network.Message.FillMsgContentMeta_String(NetMQFrame)
        }

        private void FillMsgContent(NetMQFrame frame)
        {
            // TODO: Implement Network.Message.AddMsgContent(NetMQFrame)
        }

        public void AddString(string str)
        {
            // 4 byte - length of string
            // Length bytes - the characters
            this.mMessageContent.Add(new MessageArgument(
                MessageArgumentType.Integer,
                new MessageArgumentValue { Integer = str.Length }));

            this.AddCharacterArgument(str.ToCharArray());
        }

        public void AddImage(Image<Rgba32> img)
        {
            // 4 byte - Width
            // 4 byte - Height
            // 4 byte - ModifyColors.ColorFormat

            // n byte = Width * Height * ModifyColors.ColorFormat

            this.AddIntegerArgument(img.Width);
            this.AddIntegerArgument(img.Height);

            ModifyColors.ImageManipulator.CheckAndCorrectColorFormat(img);
            var colorFormat = ImageManipulator.GetColorFormat(img);
            this.AddIntegerArgument((Int32)colorFormat);


            for (var x = 0; x < img.Width; ++x)
            {
                for (var y = 0; y < img.Height; ++y)
                {
                    var p = img[x, y];

                    switch (colorFormat)
                    {
                        case ColorFormat.Rgba32:
                            this.AddByteArgument(p.R, p.G, p.B);
                            break;
                        case ColorFormat.Grayscale:
                            this.AddByteArgument(p.R);
                            break;
                        default:
                            throw new NotSupportedException("Only RGB and Greyscale are supported");
                    }
                }
            }
        }

        private static Message NetMqMsgToStringMsg(NetMQMessage netMqMessage)
        {
            
        }

#region AddArguments

        private void UpdateLength(MessageArgumentType type)
        {
            this.mSize += sizeof(MessageArgumentType);
            this.mSize += type switch
            {
                MessageArgumentType.Boolean => sizeof(bool),
                MessageArgumentType.Byte => sizeof(byte),
                MessageArgumentType.Character => sizeof(char),
                MessageArgumentType.Short => sizeof(short),
                MessageArgumentType.Integer => sizeof(int),
                MessageArgumentType.Long => sizeof(long),
                MessageArgumentType.Float => sizeof(float),
                MessageArgumentType.Double => sizeof(double),
                MessageArgumentType.Decimal => sizeof(decimal),
                _ => throw new NotSupportedException($"The '{nameof(MessageArgumentType)}' with a value of '{(int)type}' is not supported")
            };
        }

        public void AddByteArgument(byte b)
        {
            var val = new MessageArgumentValue();
            val.Byte = b;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Byte, val));
            UpdateLength(MessageArgumentType.Byte);
        }

        public void AddByteArgument(params byte[] b)
        {
            foreach (var x in b)
            {
                AddByteArgument(x);
            }
        }

        public void AddCharacterArgument(char c)
        {
            var val = new MessageArgumentValue();
            val.Character = c;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Character, val));
            UpdateLength(MessageArgumentType.Character);
        }

        public void AddCharacterArgument(params char[] c)
        {
            foreach (var x in c)
            {
                AddCharacterArgument(x);
            }
        }

        public void AddShortArgument(short c)
        {
            var val = new MessageArgumentValue();
            val.Short = c;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Short, val));
            UpdateLength(MessageArgumentType.Short);
        }

        public void AddShortArgument(params short[] c)
        {
            foreach (var x in c)
            {
                AddShortArgument(x);
            }
        }

        public void AddIntegerArgument(int i)
        {
            var val = new MessageArgumentValue();
            val.Integer = i;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Integer, val));
            UpdateLength(MessageArgumentType.Integer);
        }

        public void AddIntegerArgument(params int[] i)
        {
            foreach (var x in i)
            {
                AddIntegerArgument(x);
            }
        }

        public void AddLongArgument(long i)
        {
            var val = new MessageArgumentValue();
            val.Long = i;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Long, val));
            UpdateLength(MessageArgumentType.Long);
        }

        public void AddLongArgument(params long[] i)
        {
            foreach (var x in i)
            {
                AddLongArgument(x);
            }
        }

        public void AddFloatArgument(float f)
        {
            var val = new MessageArgumentValue();
            val.Float = f;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Float, val));
            UpdateLength(MessageArgumentType.Float);
        }

        public void AddFloatArgument(params float[] f)
        {
            foreach (var x in f)
            {
                AddFloatArgument(x);
            }
        }

        public void AddDoubleArgument(double d)
        {
            var val = new MessageArgumentValue();
            val.Double = d;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Double, val));
            UpdateLength(MessageArgumentType.Double);
        }

        public void AddDoubleArgument(params double[] d)
        {
            foreach(var x in d)
            {
                AddDoubleArgument(x);
            }
        }

        public void AddDecimalArgument(decimal d)
        {
            var val = new MessageArgumentValue();
            val.Decimal = d;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Decimal, val));
            UpdateLength(MessageArgumentType.Decimal);
        }

        public void AddDecimalArgument(params decimal[] d)
        {
            foreach (var x in d)
            {
                AddDecimalArgument(x);
            }
        }

        public void AddBooleanArgument(bool b)
        {
            var val = new MessageArgumentValue();
            val.Boolean = b;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Boolean, val));
            UpdateLength(MessageArgumentType.Boolean);
        }

        public void AddBooleanArgument(params bool[] b)
        {
            foreach (var x in b)
            {
                AddBooleanArgument(x);
            }
        }
#endregion
    }
}