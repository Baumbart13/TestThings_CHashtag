using System.Text;

namespace Network
{
    public class MessageArgument
    {
        private readonly MessageArgumentType mType;
        private readonly MessageArgumentValue mValue;

        public MessageArgumentType Type => this.mType;
        public MessageArgumentValue Value => this.mValue;

        public MessageArgument(MessageArgumentType type, in MessageArgumentValue value)
        {
            this.mType = type;
            this.mValue = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Type);
            sb.Append(":");
            switch (this.mType)
            {
                case MessageArgumentType.Byte:
                    sb.Append(Value.Byte.ToString());
                    break;
                case MessageArgumentType.Integer:
                    sb.Append(Value.Integer.ToString());
                    break;
                case MessageArgumentType.Float:
                    sb.Append(Value.Float.ToString());
                    break;
                case MessageArgumentType.Double:
                    sb.Append(Value.Double.ToString());
                    break;
                case MessageArgumentType.Boolean:
                    sb.Append(Value.Boolean.ToString());
                    break;
                default:
                    throw new InvalidOperationException("There are no more argument types");
            }

            return sb.ToString();
        }
    }
}