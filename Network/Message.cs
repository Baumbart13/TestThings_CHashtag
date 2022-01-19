using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using ModifyColors;
using ModifyColors.Extensions;
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
        public MessageType MessageType { init; get; }

        public Message(MessageType type)
        {
            this.MessageType = type;
            this.mMessageContent = new List<MessageArgument>();
        }

        public NetMQ.NetMQMessage ToNetMqMessage()
        {
            
        }

        public void AddString(String str)
        {
            // 4 byte - length of string
            // Length bytes - the characters
            this.mMessageContent.Add(new MessageArgument(
                MessageArgumentType.Integer,
                new MessageArgumentValue { Integer = str.Length }));

            foreach(var x in str)
            {
                this.mMessageContent.Add(new MessageArgument(
                    MessageArgumentType.Character,
                    new MessageArgumentValue { Character = x }));
            }
        }

        public void AddRgbImage(Image<Rgba32> img)
        {
            // 4 byte - Width
            // 4 byte - Height
            // 4 byte - ModifyColors.PixelFormat
            // 4 byte - Bits per pixel

            // n byte = ([Bits per pixel] / 8) * Width * Height * ModifyColors.PixelFormat

            this.AddIntegerArgument(img.Width);
            this.AddIntegerArgument(img.Height);

            ModifyColors.ImageManipulator.CheckAndCorrectImageFormat(img);
            var pixelFormat =
                (ModifyColors.PixelFormat)img.GetConfiguration().Properties[nameof(ModifyColors.PixelFormat)];
            this.AddIntegerArgument((int)pixelFormat);

            var bpp = pixelFormat.GetBitsPerPixel();
            this.AddIntegerArgument((int)bpp);

            for (var x = 0; x < img.Width; ++x)
            {
                for (var y = 0; y < img.Height; ++y)
                {
                    var p = img[x, y];

                    switch (pixelFormat)
                    {
                        case PixelFormat.Rgba32:
                            this.mMessageContent.AddRange(new MessageArgument[]
                            {
                                new MessageArgument(
                                    MessageArgumentType.Byte,
                                    new MessageArgumentValue { Byte = p.R }),
                                new MessageArgument(
                                    MessageArgumentType.Byte,
                                    new MessageArgumentValue { Byte = p.G }),
                                new MessageArgument(
                                    MessageArgumentType.Byte,
                                    new MessageArgumentValue { Byte = p.B })
                            });
                            break;
                        case PixelFormat.Grayscale:
                            this.mMessageContent.Add(new MessageArgument(
                                MessageArgumentType.Byte,
                                new MessageArgumentValue { Byte = p.R }));
                            break;
                        default:
                            throw new NotImplementedException("Only RGB and Greyscale are supported");
                    }
                }
            }
        }

        public void AddByteArgument(byte b)
        {
            var val = new MessageArgumentValue();
            val.Byte = b;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Byte, val));
        }

        public void AddByteArgument(params byte[] b)
        {
            foreach (var x in b)
            {
                AddByteArgument(x);
            }
        }

        public void AddIntegerArgument(int i)
        {
            var val = new MessageArgumentValue();
            val.Integer = i;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Integer, val));
        }

        public void AddIntegerArgument(params int[] i)
        {
            foreach (var x in i)
            {
                AddIntegerArgument(x);
            }
        }

        public void AddBooleanArgument(bool b)
        {
            var val = new MessageArgumentValue();
            val.Boolean = b;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Boolean, val));
        }

        public void AddBooleanArgument(params bool[] b)
        {
            foreach (var x in b)
            {
                AddBooleanArgument(x);
            }
        }

        public void AddDoubleArgument(double d)
        {
            var val = new MessageArgumentValue();
            val.Double = d;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Double, val));
        }

        public void AddDoubleArgument(params double[] d)
        {
            foreach(var x in d)
            {
                AddDoubleArgument(x);
            }
        }

        public void AddFloatArgument(float f)
        {
            var val = new MessageArgumentValue();
            val.Float = f;
            this.mMessageContent.Add(new MessageArgument(MessageArgumentType.Float, val));
        }

        public void AddFloatArgument(params float[] f)
        {
            foreach (var x in f)
            {
                AddFloatArgument(x);
            }
        }
    }
}