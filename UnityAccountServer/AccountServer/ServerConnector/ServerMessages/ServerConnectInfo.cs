using System.Collections.Immutable;
using UnityClientServer;

public static class ServerConnectInfo
{
    private static Dictionary<ushort, IRemoteServer> connectedServers = new Dictionary<ushort, IRemoteServer>(16);
    private static Dictionary<ushort, ushort> connectedServersInternal = new Dictionary<ushort, ushort>(16);

    public static bool Save(IRemoteServer remoteServer)
    {
        bool tryAdd = connectedServers.TryAdd(remoteServer.Port, remoteServer);
        if (!tryAdd) return false;
        
        bool tryParse = Enum.TryParse(remoteServer.ServerGroup.Name.AsSpan(),
            out ServerNames name);

        if (!tryParse) return false;
            
        connectedServersInternal.Add((ushort)name, remoteServer.Port);
        return true;
    }

    public static IRemoteServer? Get(ServerNames serverName)
    {
        bool tryGetValue = connectedServersInternal.TryGetValue((ushort)serverName, out ushort port);
        
        return !tryGetValue ? null : connectedServers[port];
    }
    public static void Remove(IRemoteServer remoteServer)
    {
        if (!connectedServers.ContainsKey(remoteServer.Port)) return;

        connectedServers.Remove(remoteServer.Port);
        
        bool tryParse = Enum.TryParse(nameof(remoteServer.ServerGroup.Name),
            out ServerNames name);

        if (!tryParse || !connectedServersInternal.ContainsKey((ushort)name)) return;

        connectedServersInternal.Remove((ushort)name);
    }
    public static bool Exist(IRemoteServer remoteServer)
    {
        return connectedServers.ContainsKey(remoteServer.Port);
    }
}