using static ServerConnectInfo;

public class SignOutLogic
{
    public static void ProcessRequest(DarkRiftReader reader, ServerMessageReceivedEventArgs e)
    {
        SendMessageTo(Get(ServerNames.AccountServer), Tags.SIGN_OUT_SUCCESS, reader.ReadSerializable<SignOutRequest>());
        SendMessageTo(Get(ServerNames.AccountServer), Tags.SIGN_OUT_FAILED, reader.ReadSerializable<SignOutRequest>());
    }
}