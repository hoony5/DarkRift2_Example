using UnityClientServer;
using static ServerConnectInfo;

public class SignOutLogic
{
    public static void ProcessRequest(DarkRiftReader reader, MessageReceivedEventArgs e)
    {
        SendMessageTo(Get(ServerNames.AccountServer), Tags.SIGN_OUT_REQUEST, reader.ReadSerializable<SignOutRequest>());
    }
}