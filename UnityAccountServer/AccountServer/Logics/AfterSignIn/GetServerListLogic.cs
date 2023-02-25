using static ServerConnectInfo;

public static class GetServerListLogic
{
    public static void ProcessRequest(DarkRiftReader reader, ServerMessageReceivedEventArgs e)
    {
        GetServerListRequest req = reader.ReadSerializable<GetServerListRequest>();

        bool existAccountID = MariaDBManager.ExistDB(MariaDBData.DBTable, "ACCOUNT_ID", req.AccountID);
        
        // if true
        GetServerListSuccess success = new GetServerListSuccess()
        {
            
        };
        SendMessageTo(Get(ServerNames.AccountServer), Tags.GET_SERVER_LIST_SUCCESS, reader.ReadSerializable<GetServerListRequest>());
        
        
        // else
        GetServerListFailed failed = new GetServerListFailed()
        {
            
        };
        SendMessageTo(Get(ServerNames.AccountServer), Tags.GET_SERVER_LIST_FAILED, reader.ReadSerializable<GetServerListRequest>());
    }
}   