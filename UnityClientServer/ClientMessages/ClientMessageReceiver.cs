namespace UnityClientServer;

public static class ClientMessageReceiver
{
    private static StringBuilder _messageStringBuilder = new StringBuilder(200);
    public static void ProcessMessageFromClient(object? sender, MessageReceivedEventArgs e)
    {
        using Message message = e.GetMessage();
        using DarkRiftReader reader = message.GetReader();
        _messageStringBuilder.Clear();
        _messageStringBuilder.Append($"Recv Message {(Tags)e.Tag} | {e.Client.ID}\n");
        Console.WriteLine(_messageStringBuilder.ToString());
        switch ((Tags)e.Tag)
        {
            case Tags.None:
                break;
            case Tags.DEBUG_SEND_TO_CLIENT_RESULT:
                DebugLogic.ProcessRequest(reader,e);
                break;
            case Tags.DEBUG_SEND_TO_SERVER_REQUEST:
                DebugLogic.ProcessRequestToServer(reader, e);
                break;
            case Tags.CHECK_VERSION_REQUEST:
                CheckVersionLogic.ProcessRequest(reader, e);
                break;
            case Tags.UPDATE_VERSION_REQUEST:
                UpdateVersionLogic.ProcessRequest(reader, e);
                break;
            case Tags.GET_SERVER_LIST_REQUEST:
                GetServerListLogic.ProcessRequest(reader, e);
                break;
            case Tags.SIGN_UP_REQUEST:
                SignUpLogic.ProcessRequest(reader,e);
                break;
            case Tags.SIGN_IN_REQUEST:
                SignInLogic.ProcessRequest(reader,e);
                break;
            case Tags.SIGN_OUT_REQUEST:
                SignOutLogic.ProcessRequest(reader,e);
                break;
            case Tags.CREATE_CHARACTER_REQUEST:
                break;
            case Tags.REMOVE_CHARACTER_REQUEST:
                break;
            case Tags.ENTER_WORLD_REQUEST:
                break;
            default:
                throw new ArgumentOutOfRangeException($"Not supported tag by the Client Server *** | {e.Tag}. | ***");
        }
    }
}