using static ServerConnectInfo;

public static class SignUpLogic
{
    public static void ProcessRequest(DarkRiftReader reader, ServerMessageReceivedEventArgs e)
    {
        SendMessageTo(Get(ServerNames.AccountServer), Tags.SIGN_UP_SUCCESS, reader.ReadSerializable<SignUpRequest>());
        SendMessageTo(Get(ServerNames.AccountServer), Tags.SIGN_UP_FAILED, reader.ReadSerializable<SignUpRequest>());
    }
}