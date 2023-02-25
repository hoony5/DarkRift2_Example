namespace UnityClientServer;

public class AccountServer : Plugin
{
    public override Version Version { get; } = new Version(1, 0, 0);
    public override bool ThreadSafe { get; } = false;
    public AccountServer(PluginLoadData pluginLoadData) : base(pluginLoadData)
    {
        Logger.Info("LogicServer is running..");
    }

    protected override void Install(InstallEventArgs args)
    {
        base.Install(args);
        ConnectDB();
    }
    
    private void ConnectDB()
    {
        // connect MariaDB
        bool connectMariaDb = MariaDBManager.ConnectMariaDB();
        Logger.Info($"MariaDB is Connected ? {connectMariaDb.ToString()}");
    }
}