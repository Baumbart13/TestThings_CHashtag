using NetMQ;

namespace Network.Extensions;

public static class RequestSocketExtensions
{
    public static void SendMessage(this IOutgoingSocket socket, Message message)
    {
        socket.SendMessage(message.ToNetMqMessage());
    }

    public static IOutgoingSocket SendMoreMessages(this IOutgoingSocket socket, Message message)
    {
        
        return socket;
    }
}