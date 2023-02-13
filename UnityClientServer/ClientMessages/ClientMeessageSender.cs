namespace UnityClientServer;

public static class ClientMeessageSender
{
    private static StringBuilder _messageStringBuilder = new StringBuilder(200);
    
    public static void SendMessage<T>(IClient client, Tags tag, T type, SendMode sendMode = SendMode.Reliable) where T : IDarkRiftSerializable
    {
        using Message message = Message.Create((ushort)tag, type);
        
        bool isSuccess = client.SendMessage(message, sendMode);
        _messageStringBuilder.Clear();
        _messageStringBuilder.Append($"{tag} | {type} | Send Success ? {isSuccess}\n");
        Console.WriteLine(_messageStringBuilder.ToString());
    } 
}