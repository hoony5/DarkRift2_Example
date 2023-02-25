using static ServerConnectInfo;

public static class SignInLogic
{
    public static void ProcessRequest(DarkRiftReader reader, ServerMessageReceivedEventArgs e)
    {
        // something code..
        
        
        SendMessageTo(Get(ServerNames.AccountServer), Tags.SIGN_IN_SUCCESS, reader.ReadSerializable<SignInRequest>());
        SendMessageTo(Get(ServerNames.AccountServer), Tags.SIGN_IN_FAILED, reader.ReadSerializable<SignInRequest>());
    }
}