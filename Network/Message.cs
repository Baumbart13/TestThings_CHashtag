using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using ModifyColors;
using ModifyColors.Extensions;
using NetMQ;
using Network.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Network
{
    public class Message
    {
        private const string OtherTypesNotImplemented = "Other types are not mplemented by now";
        private readonly List<MessageArgument> mMessageContent;
        public IReadOnlyList<MessageArgument> Content => this.mMessageContent;

        private uint mSize;

        private int mContentIter = 0;

        protected int GetContentIterIndex
        {
            get
            {
                var t = mContentIter;
                mContentIter += 1;
                if (mContentIter % 10 == 0)
                {
                    Console.WriteLine($"mContentIter is {mContentIter}");
                }

                return t;
            }
        }

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
            // it's a stack.. damn
            var netMqContainer = netMqMsg[2];
            var netMqMeta = netMqMsg[1];
            var netMqContent = netMqMsg[0];

            var msgType = IdentifyMessageType(netMqContainer);
            var msgMeta = IdentifyMessageMeta(msgType, netMqMeta);
            var msgContent = IdentifyMessageContent(msgType, msgMeta, netMqContent);

            var msg = new Message(msgType);
            msg.mMessageContent.Capacity = msgMeta.Count + msgContent.Count;

            // insert meta
            for (var i = 0; i < msgMeta.Count; ++i)
            {
                var currArg = msgMeta[i];
                switch (currArg.Type)
                {
                    case MessageArgumentType.Boolean:
                        msg.AddBooleanArgument(currArg.Value.Boolean);
                        break;
                    case MessageArgumentType.Byte:
                        msg.AddByteArgument(currArg.Value.Byte);
                        break;
                    case MessageArgumentType.Short:
                        msg.AddShortArgument(currArg.Value.Short);
                        break;
                    case MessageArgumentType.Character:
                        msg.AddCharacterArgument(currArg.Value.Character);
                        break;
                    case MessageArgumentType.Integer:
                        msg.AddIntegerArgument(currArg.Value.Integer);
                        break;
                    case MessageArgumentType.Long:
                        msg.AddLongArgument(currArg.Value.Long);
                        break;
                    case MessageArgumentType.Float:
                        msg.AddFloatArgument(currArg.Value.Float);
                        break;
                    case MessageArgumentType.Double:
                        msg.AddDoubleArgument(currArg.Value.Double);
                        break;
                    case MessageArgumentType.Decimal:
                        msg.AddDecimalArgument(currArg.Value.Decimal);
                        break;
                    default:
                        throw new NotSupportedException($"The '{nameof(MessageArgumentType)}' with a value of '{(int)currArg.Type}' is not supported");
                }
            }
            
            // Insert content
            for (var i = 0; i < msgContent.Count; ++i)
            {
                var currArg = msgContent[i];
                switch (currArg.Type)
                {
                    case MessageArgumentType.Boolean:
                        msg.AddBooleanArgument(currArg.Value.Boolean);
                        break;
                    case MessageArgumentType.Byte:
                        msg.AddByteArgument(currArg.Value.Byte);
                        break;
                    case MessageArgumentType.Short:
                        msg.AddShortArgument(currArg.Value.Short);
                        break;
                    case MessageArgumentType.Character:
                        msg.AddCharacterArgument(currArg.Value.Character);
                        break;
                    case MessageArgumentType.Integer:
                        msg.AddIntegerArgument(currArg.Value.Integer);
                        break;
                    case MessageArgumentType.Long:
                        msg.AddLongArgument(currArg.Value.Long);
                        break;
                    case MessageArgumentType.Float:
                        msg.AddFloatArgument(currArg.Value.Float);
                        break;
                    case MessageArgumentType.Double:
                        msg.AddDoubleArgument(currArg.Value.Double);
                        break;
                    case MessageArgumentType.Decimal:
                        msg.AddDecimalArgument(currArg.Value.Decimal);
                        break;
                    default:
                        throw new NotSupportedException($"The '{nameof(MessageArgumentType)}' with a value of '{(int)currArg.Type}' is not supported");
                }
            }

            return msg;
        }

        /// <summary>
        /// Adds the message type from NetMQFrame to an Artemis-message
        /// </summary>
        /// <param name="container">The first frame of a NetMQMessage</param>
        /// <returns>The MessageType of the NetMQMessage</returns>
        private static MessageType IdentifyMessageType(NetMQFrame container)
        {
            var bArrMsgType = container.Buffer;
            var iMsgType = bArrMsgType.ToInt32();
            return (MessageType)iMsgType;
        }

        /// <summary>
        /// Adds the metadata of the content, like size of the packet, width and height of the image inside of the packet
        /// from a NetMQFrame to an Artemis-message
        /// </summary>
        /// <param name="type">The MessageType previously found in the first NetMQFrame of the NetMQMessage</param>
        /// <param name="meta">The second frame of a NetMQMessage</param>
        /// <returns>The metadata of the content</returns>
        /// <exception cref="NotImplementedException">Not all types of Messages are implemented</exception>
        private static IList<MessageArgument> IdentifyMessageMeta(MessageType type, NetMQFrame meta)
        {
            return type switch
            {
                MessageType.Image => IdentifyMessageMeta_Image(meta),
                MessageType.String => IdentifyMessageMeta_String(meta),
                _ => throw new NotImplementedException(OtherTypesNotImplemented)
            };
        }
        
#region Identify message content meta

        private static IList<MessageArgument> IdentifyMessageMeta_Image(NetMQFrame meta)
        {
            // ordered like
            //
            // Width       -> 4 byte
            // Height      -> 4 byte
            // ColorFormat -> 4 byte
            var bArrWidth = new byte[4];
            var bArrHeight = new byte[4];
            var bArrColFormat = new byte[4];
            var i = 0;
            Array.Copy(meta.Buffer, i, bArrWidth, 0, bArrWidth.Length);
            i += bArrWidth.Length;
            Array.Copy(meta.Buffer, i, bArrHeight, 0, bArrHeight.Length);
            i += bArrHeight.Length;
            Array.Copy(meta.Buffer, i, bArrColFormat, 0, bArrColFormat.Length);
            
            // Convert byte-arrays to int and into MessageArgument
            var iWidth = bArrWidth.ToInt32();
            var iHeight = bArrHeight.ToInt32();
            var iColFormat = bArrColFormat.ToInt32();
            
            var l = new List<MessageArgument>(3)
            {
                new MessageArgument(
                    MessageArgumentType.Integer,
                    new MessageArgumentValue{Integer = iWidth}),
                new MessageArgument(
                    MessageArgumentType.Integer,
                    new MessageArgumentValue{Integer = iHeight}),
                new MessageArgument(
                    MessageArgumentType.Integer,
                    new MessageArgumentValue{Integer = iColFormat})
            };

            return l;
        }

        private static IList<MessageArgument> IdentifyMessageMeta_String(NetMQFrame meta)
        {
            throw new NotImplementedException();
        }

#endregion

        /// <summary>
        /// Adds the content inside of a NetMQFrame to an Artemis-message
        /// </summary>
        /// <param name="type">The MessageType previously found in the first NetMQFrame of the NetMQMessage</param>
        /// <param name="meta">The metadata previously found in the second NetMQFrame of the NetMQMessage</param>
        /// <param name="content">The final and third frame of a NetMQFrame</param>
        /// <returns>The actual content of the message</returns>
        /// <exception cref="NotImplementedException">Not all type of Messages are implemented</exception>
        private static IList<MessageArgument> IdentifyMessageContent(MessageType type, IList<MessageArgument> meta,  NetMQFrame content)
        {
            return type switch
            {
                MessageType.Image => IdentifyMessageContent_Image(meta, content),
                MessageType.String => IdentifyMessageContent_String(meta, content),
                _ => throw new NotImplementedException(OtherTypesNotImplemented)
            };
        }

#region Identify message content

        private static IList<MessageArgument> IdentifyMessageContent_Image(IList<MessageArgument> meta, NetMQFrame content)
        {
            // index == 0; Width
            // index == 1; Height
            // index == 2; ColorFormat
            double bytesPerPixel = meta[2].Value.Integer;
            
            var l = new List<MessageArgument>((int)Math.Ceiling(content.BufferSize / bytesPerPixel));
            for (var i = 0; i < content.BufferSize; ++i)
            {
                var pixel = new MessageArgument(
                    MessageArgumentType.Byte,
                    new MessageArgumentValue { Byte = content.Buffer[i] });
                l.Add(pixel);
            }

            return l;
        }

        private static IList<MessageArgument> IdentifyMessageContent_String(IList<MessageArgument> meta, NetMQFrame content)
        {
            throw new NotImplementedException();
        }

#endregion

        public NetMQ.NetMQMessage ToNetMqMessage()
        {
            var netmqMsg = new NetMQMessage();

            // 4 bytes to hold a 32-bit int aka enum
            //var container = new NetMQFrame(sizeof(MessageType));
            var container = new NetMQFrame(4);
            
            // Amount of bytes needed for metadata of the message-content
            var msgMeta = new NetMQFrame(this.MessageType switch
            {
                MessageType.Image => 4 + 4 + 4, // ColorFormat, Height, Width
                MessageType.ImageRequest => 0,
                MessageType.String => 4, // string-length
                _ => throw new NotImplementedException(OtherTypesNotImplemented)
            });
            
            // Amount of bytes needed for the actual content of message
            var msg = new NetMQFrame(this.MessageType switch
            {
                MessageType.Image => this.mMessageContent.Count-3,
                MessageType.ImageRequest => 0,
                MessageType.String => this.mMessageContent.Count * 2, // a character needs 2 bytes in .NET
                _ => throw new NotImplementedException(OtherTypesNotImplemented)
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
            var bArrMsgType = iMsgType.ToBytes();
            
            bArrMsgType.CopyTo(container.Buffer, 0);
        }

        /// <summary>
        /// Adds the metadata of the content, like size of the packet, width and height of the image inside of the packet.
        /// </summary>
        /// <param name="frame"></param>
        private void FillMsgContentMeta(NetMQFrame frame)
        {
            switch (this.MessageType)
            {
                case MessageType.Image:
                    FillMsgContentMeta_Image(frame);
                    break;
                case MessageType.String:
                    FillMsgContentMeta_String(frame);
                    break;
                default:
                    throw new NotImplementedException(OtherTypesNotImplemented);
                    break;
            }
        }

#region Fill message content meta

        private void FillMsgContentMeta_Image(NetMQFrame frame)
        {
            // ordered like
            //
            // Width       -> 4 byte
            // Height      -> 4 byte
            // ColorFormat -> 4 byte
            var iWidth = this.mMessageContent[GetContentIterIndex].Value.Integer;
            var bArrWidth = iWidth.ToBytes();
            var iHeight = this.mMessageContent[GetContentIterIndex].Value.Integer;
            var bArrHeight = iHeight.ToBytes();
            var iColFormat = this.mMessageContent[GetContentIterIndex].Value.Integer;
            var bArrColFormat = iColFormat.ToBytes();

            var arrCombined = new byte[bArrWidth.Length + bArrHeight.Length + bArrColFormat.Length];
            var i = 0;
            Array.Copy(bArrWidth, 0, frame.Buffer, i, bArrWidth.Length);
            i += bArrWidth.Length;
            Array.Copy(bArrHeight, 0, frame.Buffer, i, bArrHeight.Length);
            i += bArrHeight.Length;
            Array.Copy(bArrColFormat, 0, frame.Buffer, i, bArrColFormat.Length);
        }

        private void FillMsgContentMeta_String(NetMQFrame frame)
        {
            // TODO: Implement Network.Message.FillMsgContentMeta_String(NetMQFrame)
        }
#endregion

        private void FillMsgContent(NetMQFrame frame)
        {
            switch (this.MessageType)
            {
                case MessageType.Image:
                    FillMsgContent_Image(frame);
                    break;
                case MessageType.String:
                    FillMsgContent_String(frame);
                    break;
                default:
                    throw new NotImplementedException(OtherTypesNotImplemented);
                    break;
            }

            this.mContentIter = 0;
        }

#region Fill Message content
        private void FillMsgContent_Image(NetMQFrame frame)
        {
            for (var i = 0; i < frame.BufferSize; ++i)
            {
                frame.Buffer[i] = this.mMessageContent[GetContentIterIndex].Value.Byte;
            }
        }

        private void FillMsgContent_String(NetMQFrame frame)
        {
            // TODO: Implement Network.Message.FillMsgContent_String(NetMQFrame)
        }
#endregion

#region Add bigger objects
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

        private static Message NetMqMsgToStringMsg(NetMQMessage netMqMessage)
        {
            throw new NotImplementedException();
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