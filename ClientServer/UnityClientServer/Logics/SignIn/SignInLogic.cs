using UnityClientServer;
using static ServerConnectInfo;

public static class SignInLogic
{
    public static void ProcessRequest(DarkRiftReader reader, MessageReceivedEventArgs e)
    {
        SendMessageTo(Get(ServerNames.AccountServer), Tags.SIGN_IN_REQUEST, reader.ReadSerializable<SignInRequest>());
    }
}