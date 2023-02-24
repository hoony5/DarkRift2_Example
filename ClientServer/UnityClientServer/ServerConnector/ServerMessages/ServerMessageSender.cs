public static class ServerMessageSender
{
    private static StringBuilder _messageStringBuilder = new StringBuilder(200);
    
    public static void SendMessageTo<T>(IRemoteServer? remoteServer, Tags tag, T type, SendMode sendMode = SendMode.Reliable) where T : IDarkRiftSerializable
    {
        using Message message = Message.Create((ushort)tag, type);
        
        bool isSuccess = remoteServer is not null && remoteServer.SendMessage(message, sendMode);
        _messageStringBuilder.Clear();
        _messageStringBuilder.Append($"{tag} | {type} | Send Success ? {isSuccess}\n");
        Console.WriteLine(_messageStringBuilder.ToString());
    } 
}