using UnityClientServer;
using static ServerConnectInfo;

public static class CheckVersionLogic
{
    public static void ProcessRequest(DarkRiftReader reader, MessageReceivedEventArgs e)
    {
        // check file server version
        SendMessageTo(Get(ServerNames.FileServer), Tags.CHECK_VERSION_REQUEST, reader.ReadSerializable<CheckVersionRequest>());
    }
}