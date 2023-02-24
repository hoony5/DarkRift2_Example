public static class ServerMessageReceiver
{
    private static StringBuilder _messageStringBuilder = new StringBuilder(200);
    public static void ProcessMessageFromServer(object? sender, ServerMessageReceivedEventArgs e)
    {
        using Message message = e.GetMessage();
        using DarkRiftReader reader = message.GetReader();
        
        _messageStringBuilder.Clear();
        _messageStringBuilder.Append($"Recv Message From Server {(Tags)e.Tag} | {e.RemoteServer.ServerGroup} | {e.RemoteServer.Port}\n");
        Console.WriteLine(_messageStringBuilder.ToString());
        switch ((Tags)e.Tag)
        {
            case Tags.DEBUG_SEND_TO_SERVER_RESULT:
                break;
            case Tags.CHECK_VERSION_SUCCESS:
                break;
            case Tags.CHECK_VERSION_FAILED:
                break;
            case Tags.UPDATE_VERSION_SUCCESS:
                break;
            case Tags.UPDATE_VERSION_FAILED:
                break;
            case Tags.GET_SERVER_LIST_SUCCESS:
                break;
            case Tags.GET_SERVER_LIST_FAILED:
                break;
            case Tags.SIGN_UP_SUCCESS:
                break;
            case Tags.SIGN_UP_FAILED:
                break;
            case Tags.SIGN_IN_SUCCESS:
                break;
            case Tags.SIGN_IN_FAILED:
                break;
            case Tags.SIGN_OUT_SUCCESS:
                break;
            case Tags.SIGN_OUT_FAILED:
                break;
            case Tags.CREATE_CHARACTER_SUCCESS:
                break;
            case Tags.CREATE_CHARACTER_FAILED:
                break;
            case Tags.REMOVE_CHARACTER_SUCCESS:
                break;
            case Tags.REMOVE_CHARACTER_FAILED:
                break;
            case Tags.ENTER_WORLD_SUCCESS:
                break;
            case Tags.ENTER_WORLD_FAILED:
                break;
            default:
                throw new ArgumentOutOfRangeException($"Not supported tag by the Client Server {e.Tag}");
        }
    }
}