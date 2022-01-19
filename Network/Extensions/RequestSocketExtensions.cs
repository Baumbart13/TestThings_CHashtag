using NetMQ;

namespace Network.Extensions;

public static class RequestSocketExtensions
{
    public static void SendMessage(this IOutgoingSocket socket, Message message)
    {
        // - MessageType
        // - Number of MessageArgument
        // - for each MessageArgument
        // --- MessageArgumentType
        // --- MessageArgumentValue

        Int32 msgType = (Int32)message.MessageType;
        Int32 msgNoArgs = (Int32)message.Content.Count;
        NetMQ.NetMQMessage msg = new NetMQMessage();
        msg.AppendEmptyFrame();
        msg[0].
    }

    public static IOutgoingSocket SendMoreMessages(this IOutgoingSocket socket, Message message)
    {
        
        return socket;
    }
}