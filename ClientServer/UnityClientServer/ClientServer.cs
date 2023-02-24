namespace UnityClientServer;

public class ClientServer : Plugin
{
    public override Version Version { get; } = new Version(1, 0, 0);
    public override bool ThreadSafe { get; } = false;
    public ClientServer(PluginLoadData pluginLoadData) : base(pluginLoadData)
    {
        Logger.Info("ClientServer is running..");

        ClientManager.ClientConnected += OnClientConnected;
        ClientManager.ClientDisconnected += OnClientDisconnected;
    }
    
    private void OnRecvMessage(object? sender, MessageReceivedEventArgs e)
    {
        ClientMessageReceiver.ProcessMessageFromClient(sender, e);
    }

    private void OnClientConnected(object? sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += OnRecvMessage;
        Logger.Info($"Client[{e.Client.ID}] is Connected...");
    }
    private void OnClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
    {
        e.Client.MessageReceived -= OnRecvMessage;
        Logger.Info($"Client[{e.Client.ID}] is disconnected...");
    }
}