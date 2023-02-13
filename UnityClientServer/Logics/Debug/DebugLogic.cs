using UnityClientServer;
using UnityClientServerModel.Models;

public static class DebugLogic
{
    public static void ProcessRequest(DarkRiftReader reader, MessageReceivedEventArgs e)
    {
        DebugModelRequest req = reader.ReadSerializable<DebugModelRequest>();
        DebugModelResult result = new DebugModelResult
        {
            Tags = req.Tags,
            Result = $" Recv : Client Server | {req.Log}."
        };
        // return to client
        ClientMeessageSender.SendMessage(e.Client, Tags.DEBUG_SEND_TO_CLIENT_RESULT, result);
    }
    public static void ProcessRequestToServer(DarkRiftReader reader, MessageReceivedEventArgs e)
    {
        DebugModelRequest req = reader.ReadSerializable<DebugModelRequest>();
        DebugModelResult result = new DebugModelResult
        {
            Tags = req.Tags,
            Result = $" Recved | {req.Log}."
        };
        bool tryParse = Enum.TryParse(req.ServerName.AsSpan(), out ServerNames serverName);
        
        if(!tryParse) Console.WriteLine($"{req.ServerName} is not contains ServerData - ServerNames : Enum");
        // send to server
        ServerMessageSender.SendMessageTo(ServerConnectInfo.Get(serverName), Tags.DEBUG_SEND_TO_SERVER_REQUEST, result);
    }
}