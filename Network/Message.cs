using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

        public static Message FromNetMqMessage(NetMQMessage netMqMsg)
        {
            var msg = new Message(MessageType.Image);
            

            return msg;
        }

        public NetMQ.NetMQMessage ToNetMqMessage()
        {
            var netmqMsg = new NetMQMessage();
            
            var containerFrame = CreateContainerFrame();
            
            var messageFrame = CreateMessageFrame();
            /*var msg = new NetMQFrame(this.MessageType switch
            {
                MessageType.Image => this.mMessageContent.Count,
                MessageType.ImageRequest => 0,
                _ => throw new NotImplementedException("Other types are not implemented by now")
            });*/
            var contentFrame = CreateContentFrame();

            netmqMsg.Append(containerFrame);
            netmqMsg.Append(messageFrame);
            netmqMsg.Append(contentFrame);

            return netmqMsg;
        }

        private NetMQFrame CreateContainerFrame()
        {
            // MessageType
            var b = new byte[]
            {
                (byte) ((int) this.MessageType >> 24),
                (byte) ((int) this.MessageType >> 16),
                (byte) ((int) this.MessageType >> 8),
                (byte) this.MessageType
            };
            return new NetMQFrame(b);
        }

        private NetMQFrame CreateMessageFrame()
        {
            // Basic arguments of message
            var messageFrame = new NetMQFrame(this.MessageType switch
            {
                MessageType.Image => CreateMessageFrame_Image(),
                MessageType.ImageRequest => CreateMessageFrame_ImageRequest(),
                _ => throw new NotImplementedException("Other types are not implemented by now")
            });
            return messageFrame;
        }
        
#region MessageFrames

        private byte[] CreateMessageFrame_Image()
        {
            var width = Content[0].Value.Integer;
            var height = Content[1].Value.Integer;
            var colorFormat = Content[2].Value.Integer;
            // 4 byte ColorFormat + 4 byte Height + 4 byte Width
            var b = new byte[]
            {
                // Width
                (byte)(width >> 24),
                (byte)(width >> 16),
                (byte)(width >> 8),
                (byte)(width),
                
                // Height
                (byte)(height >> 24),
                (byte)(height >> 16),
                (byte)(height >> 8),
                (byte)(height),
                
                // ColorFormat
                (byte)(colorFormat >> 24),
                (byte)(colorFormat >> 16),
                (byte)(colorFormat >> 8),
                (byte)(colorFormat)
            };
            return b;
        }

        private byte[] CreateMessageFrame_ImageRequest()
        {
            return Array.Empty<byte>(); // information already given by tpe of message. no more bytes needed
        }
        
#endregion

        private NetMQFrame CreateContentFrame()
        {
            // Sepcific arguments of message
            var messageFrame = new NetMQFrame(this.MessageType switch
            {
                MessageType.Image => CreateContentFrame_Image(),
                MessageType.ImageRequest => CreateContentFrame_ImageRequest(),
                _ => throw new NotImplementedException("Other types are not implemented by now")
            });
            return messageFrame;
        }
        
#region ContentFrames

        private byte[] CreateContentFrame_Image()
        {
            var b = new byte[Content.Count - 3];
            // Read every pixel
            // 1 byte Red + 1 byte Green + 1 byte Blue
            for (var i = 3; i < Content.Count; ++i)
            {
                var ArgColorChannel = Content[i];
                if (ArgColorChannel.Type != MessageArgumentType.Byte)
                {
                    throw new ArgumentException("Wrong type of variable is being read!");
                }

                b[i - 3] = ArgColorChannel.Value.Byte;
            }

            return b;
        }

        private byte[] CreateContentFrame_ImageRequest()
        {
            return Array.Empty<byte>(); // information already given by tpe of message. no more bytes needed
        }

#endregion
#region Add Content

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
        
#endregion
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