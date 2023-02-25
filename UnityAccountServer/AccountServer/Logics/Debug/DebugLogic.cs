using UnityClientServer;
using UnityClientServerModel.Models;

/// <summary>
/// Debug Message of ClientServer from Client to Any Server So, ServerMessageReceivedEventArgs no necessary.
/// </summary>
public static class DebugLogic
{
    public static void ProcessRequestToServer(DarkRiftReader reader, ServerMessageReceivedEventArgs e)
    {
        DebugModelRequest req = reader.ReadSerializable<DebugModelRequest>();
        DebugModelResult result = new DebugModelResult
        {
            Tags = req.Tags,
            Result = $" Received | {req.Log} {e.RemoteServer.Port}:{e.RemoteServer.Port} | {e.RemoteServer.ServerGroup.Name}."
        };
        bool tryParse = Enum.TryParse(req.ServerName.AsSpan(), out ServerNames serverName);
        
        if(!tryParse) Console.WriteLine($"{req.ServerName} is not contains ServerData - ServerNames : Enum");
        // send to server
        ServerMessageSender.SendMessageTo(ServerConnectInfo.Get(serverName), Tags.DEBUG_SEND_TO_SERVER_REQUEST, result);
    }
}