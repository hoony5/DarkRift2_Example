using UnityClientServer;
using static ServerConnectInfo;

public static class SignUpLogic
{
    public static void ProcessRequest(DarkRiftReader reader, MessageReceivedEventArgs e)
    {
        SendMessageTo(Get(ServerNames.AccountServer), Tags.SIGN_UP_REQUEST, reader.ReadSerializable<SignUpRequest>());
    }
}