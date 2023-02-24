using UnityClientServer;
using static ServerConnectInfo;

public static class GetServerListLogic
{
    public static void ProcessRequest(DarkRiftReader reader, MessageReceivedEventArgs e)
    {
        SendMessageTo(Get(ServerNames.AccountServer), Tags.GET_SERVER_LIST_REQUEST, reader.ReadSerializable<GetServerListRequest>());
    }
}   