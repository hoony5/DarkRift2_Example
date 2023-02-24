using UnityClientServer;
using static ServerConnectInfo;

public static class UpdateVersionLogic
{
    public static void ProcessRequest(DarkRiftReader reader, MessageReceivedEventArgs e)
    {
        // check file server version
        SendMessageTo(Get(ServerNames.FileServer), Tags.UPDATE_VERSION_REQUEST, reader.ReadSerializable<UpdateVersionRequest>());
    }
}