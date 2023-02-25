
using Consul;
using static ServerConnectInfo;
/// <summary>
/// DarkRift ServerRegistryConnector plugin for Consul.
/// </sumamry>
public class ConsulServerRegistryConnector : ServerRegistryConnector
{
    public override Version Version => new Version(1, 0, 0);
    public override bool ThreadSafe => true;

    private static StringBuilder _stringBuilder = new StringBuilder(200);
    private ServerPortInfo _serverPortInfo;
    private readonly ushort _serverPort = 4298;
    private readonly ushort _healthCheckPort = 7000;
    private readonly string _localhost = "localhost";
    /// <summary>
    /// The service name to register as in Consul.
    /// </summary>
    private readonly string _serviceName = "ProjectA";
    
    /// <summary>
    /// The URL to set as the Consul health check for the server.
    /// </summary>
    private string healthCheckUrl;
    public string HealthCheckUrl
    { 
       get => $"http" + $"://{_localhost}:{_serverPortInfo.healthCheckPort}/health";
       set => healthCheckUrl = value;
    } 

    /// <summary> 
    /// The poll interval of the Consul health check for the server.
    /// </summary>
    private readonly TimeSpan _healthCheckPollInterval = TimeSpan.FromMilliseconds(5000);

    /// <summary>
    /// The maximum time the Consul health check for the server can be failing for before the server is deregistered.
    /// </summary>
    /// <remarks>
    /// Minimuim 1m, granularity ~30 seconds.
    /// </remarks>
    private readonly TimeSpan _healthCheckTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    ///     The client to connect to Consul via.
    /// </summary>
    private readonly ConsulClient _client;

    public ConsulServerRegistryConnector(ServerRegistryConnectorLoadData pluginLoadData) : base(pluginLoadData)
    {
        _serverPortInfo = new ServerPortInfo(_healthCheckPort, _serverPort);
        
        _client = new ConsulClient(configuration =>
        {
            if (pluginLoadData.Settings["consulAddress"] != null)
                configuration.Address = new Uri(pluginLoadData.Settings["consulAddress"]);

            if (pluginLoadData.Settings["consulDatacenter"] != null)
                configuration.Datacenter = pluginLoadData.Settings["consulDatacenter"];

            if (pluginLoadData.Settings["consulToken"] != null)
                configuration.Token = pluginLoadData.Settings["consulToken"];
        });

        if (pluginLoadData.Settings["healthCheckUrl"] != null)
            HealthCheckUrl = pluginLoadData.Settings["healthCheckUrl"]; 

        if (pluginLoadData.Settings["healthCheckPollIntervalMs"] != null)
            _healthCheckPollInterval =
                TimeSpan.FromMilliseconds(int.Parse(pluginLoadData.Settings["healthCheckPollIntervalMs"]));

        if (pluginLoadData.Settings["healthCheckTimeoutMs"] != null)
        {
            _healthCheckTimeout = TimeSpan.FromMilliseconds(int.Parse(pluginLoadData.Settings["healthCheckTimeoutMs"]));
            if (_healthCheckTimeout < TimeSpan.FromMinutes(1))
                throw new InvalidOperationException("healthCheckTimeout property cannot be less than 1 minute.");
        }

        if (pluginLoadData.Settings["serviceName"] != null)
            _serviceName = pluginLoadData.Settings["serviceName"];
    }

    /// <summary>
    /// Pulls the current services from Consul and updates the server with any changes to that list.
    /// </summary>
    /// <returns>A task object for the operation.</returns>
    protected async Task FetchServices()
    {
        Logger.Trace($"Refreshing services (I'm server {RemoteServerManager.ServerID}).");

        // Query Consul for the current list of services
        // TODO the library we use doesn't seem to allow us to get only services with a passing health check
        QueryResult<CatalogService[]> result;
        try
        {
            result = await _client.Catalog.Service(_serviceName);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to fetch services from Consul as an exception occurred.", e);
            return;
        }

        CatalogService[] services = result.Response;

        // Map to ushort IDs
        Dictionary<ushort, CatalogService> parsedServices =
            services.ToDictionary(s => ushort.Parse(s.ServiceID), s => s);

        // Get all known sevices
        // TODO if a server isn't in a group we're connected with then they're not retured here which causes us to perpetually discover them
        IEnumerable<ushort> knownServices = RemoteServerManager.GetAllGroups().SelectMany(g => g.GetAllRemoteServers())
            .Select(s => s.ID);

        // Diff the current services aginst the known services
        IEnumerable<ushort> joined, left;
        joined = parsedServices.Keys.Where(k => !knownServices.Contains(k));
        left = knownServices.Where(k => !parsedServices.ContainsKey(k));

        JoinRemoteServer(parsedServices, joined);
        
        RegistServerParticipationEvent();
    }

    private void JoinRemoteServer(Dictionary<ushort, CatalogService> parsedServices, IEnumerable<ushort> joined)
    {
        foreach (ushort joinedID in joined)
        {
            CatalogService service = parsedServices[joinedID];
            string group = service.ServiceTags.First().Substring(6);
            HandleServerJoin(joinedID, group, service.Address, (ushort)service.ServicePort, service.ServiceMeta);
        }
    }

    private void RegistServerParticipationEvent()
    {
        IServerGroup[]? allGroups = RemoteServerManager.GetAllGroups();
        
        if (allGroups.Length == 0) 
            return;

        foreach (IServerGroup group in allGroups)
        {
           RegistServerJoinedEvent(group, OnGroupServerJoined);
           RegistServerLeftEvent(group, OnGroupServerLeft);
            IRemoteServer[]? allRemoteServers = group.GetAllRemoteServers();
            if(allRemoteServers.Length == 0)
                continue;

            foreach (IRemoteServer remoteServer in allRemoteServers)
            {
                bool tryAdd = Save(remoteServer);
                if(!tryAdd) Logger.Warning($"| Already Exist | {remoteServer.Port} | {remoteServer.ServerGroup.Name} |");
                // 이렇게 작성해야 알맞게 작동함
                RegistRemoteServerConnectedEvent(remoteServer, OnRemoteServerConnected);
                RegistRemoteServerDisconnectedEvent(remoteServer, OnRemoteServerDisconnected);
                RegistRemoteServerRecvMessageEvent(remoteServer, OnRemoteServerMessageReceived);
              
                // 작동순서 joined => remote Connect > .... > remote Disconnect > left
            }
        }
    }

    #region TODO :: Clean
    private void RegistServerJoinedEvent(IServerGroup group, EventHandler<ServerJoinedEventArgs> e)
    {
        group.ServerJoined -= e;
        group.ServerJoined += e;
    }
    private void RegistServerLeftEvent(IServerGroup group, EventHandler<ServerLeftEventArgs> e)
    {
        group.ServerLeft -= e;
        group.ServerLeft += e;
    }
    private void RegistRemoteServerConnectedEvent(IRemoteServer remoteServer, EventHandler<ServerConnectedEventArgs> e)
    {
        remoteServer.ServerConnected -= e;
        remoteServer.ServerConnected += e;
    }
    private void RegistRemoteServerDisconnectedEvent(IRemoteServer remoteServer, EventHandler<ServerDisconnectedEventArgs> e)
    {
        remoteServer.ServerDisconnected -= e;
        remoteServer.ServerDisconnected += e;
    }
    private void RegistRemoteServerRecvMessageEvent(IRemoteServer remoteServer, EventHandler<ServerMessageReceivedEventArgs> e)
    {
        remoteServer.MessageReceived -= e;
        remoteServer.MessageReceived += e;
    }

    private void OnGroupServerJoined(object? sender, ServerJoinedEventArgs e)
    {
        Logger.Info($"( {e.ServerGroup.Name} | {e.RemoteServer.Host}:{e.RemoteServer.Port} | id : {e.RemoteServer.ID} ) is joined");

        RegistServerJoinedEvent(e.ServerGroup, OnJoinedMessage);
        RegistServerLeftEvent(e.ServerGroup, OnGroupServerLeft);
        
        Logger.Info($"OnGroupServerJoined | Address - {e.RemoteServer.Host}:{e.RemoteServer.Port} | State - {e.RemoteServer.ConnectionState} | Group Name - {e.RemoteServer.ServerGroup.Name} | Direction - {e.RemoteServer.ServerConnectionDirection} | ID - {e.RemoteServer.ID} |");
        
        RegistRemoteServerRecvMessageEvent(e.RemoteServer, OnRemoteServerMessageReceived);
    }

    private void OnJoinedMessage(object? sender, ServerJoinedEventArgs e)
    {
        Logger.Info($"( {e.ServerGroup.Name} | {e.RemoteServer.Host}:{e.RemoteServer.Port} | id : {e.RemoteServer.ID} ) is new joined");
    }
    
    private void OnRemoteServerConnected(object? sender, ServerConnectedEventArgs e)
    {
        Logger.Info($"OnJoinedMessage | Address - {e.RemoteServer.Host}:{e.RemoteServer.Port} | State - {e.RemoteServer.ConnectionState} | Group Name - {e.RemoteServer.ServerGroup.Name} | Direction - {e.RemoteServer.ServerConnectionDirection} | ID - {e.RemoteServer.ID} |");
        
        Logger.Info($"| This Port (:{_serverPortInfo.serverPort}) Send Message To | {e.RemoteServer.Host} : {e.RemoteServer.Port} |");
    }

    private void OnRemoteServerMessageReceived(object? sender, ServerMessageReceivedEventArgs e)
    {
       ServerMessageReceiver.ProcessMessageFromServer(sender, e);
    }

    private void OnRemoteServerDisconnected(object? sender, ServerDisconnectedEventArgs e)
    {
        Logger.Info($"OnRemoteServerDisconnected | Address - {e.RemoteServer.Host}:{e.RemoteServer.Port} | State - {e.RemoteServer.ConnectionState}| Group Name - {e.RemoteServer.ServerGroup.Name} | Direction - {e.RemoteServer.ServerConnectionDirection} | ID - {e.RemoteServer.ID} |");
        e.RemoteServer.MessageReceived -= OnRemoteServerMessageReceived;
        
        Remove(e.RemoteServer);
    }

    private void OnGroupServerLeft(object? sender, ServerLeftEventArgs e)
    {
        Logger.Info($"OnGroupServerLeft | Address - {e.RemoteServer.Host}:{e.RemoteServer.Port} | State - {e.RemoteServer.ConnectionState}| Group Name - {e.RemoteServer.ServerGroup.Name} | Direction - {e.RemoteServer.ServerConnectionDirection} | ID - {e.RemoteServer.ID} |");
        e.ServerGroup.ServerJoined -= OnJoinedMessage;
        e.ServerGroup.ServerLeft -= OnLeftMessage;
        e.RemoteServer.MessageReceived -= ServerMessageReceiver.ProcessMessageFromServer;
        
        HandleServerLeave(e.RemoteServer.ID);
        Remove(e.RemoteServer);
    }

    private void OnLeftMessage(object sender, ServerLeftEventArgs e)
    {
        Logger.Info($"OnLeftMessage | ( {e.ServerGroup.Name} | {e.RemoteServer.Host}:{e.RemoteServer.Port} | id : {e.RemoteServer.ID} ) is left");
    }

    protected override void DeregisterServer()
    {
        DeregisterServerAsync(RemoteServerManager.ServerID.ToString()).Wait();
    }
    #endregion

    /// <summary>
    /// Deregisters the server with Consul.
    /// </summary>
    /// <returns>A task object for the operation.</returns>
    private async Task DeregisterServerAsync(string ushortToString)
    {
        try
        {
            await _client.Agent.ServiceDeregister(ushortToString);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to deregister server from Consul as an exception occurred.", e);
        }
    }
    
    //TODO in future, when supported by the core DarkRift libraries, this should proably be an async method
    protected override ushort RegisterServer(string group, string host, ushort port,
        IDictionary<string, string> properties)
    {
        return RegisterServerAsync(group, host, _serverPortInfo.serverPort, properties).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Registers the server with Consul.
    /// </summary>
    /// <param name="group">The group the server is in.</param>
    /// <param name="host">The advertised host property of the server.</param>
    /// <param name="port">The advertised port property of the server.</param>
    /// <param name="properties">Additional properties supplied by the server.</param>
    /// <returns>A task object for the operation.</returns>
    private async Task<ushort> RegisterServerAsync(string group, string host, int port,
        IDictionary<string, string> properties)
    {
        bool init = await InitKV();

        if (!init) await Task.Delay(1000);
        
        ushort id = await AllocateID();

        Logger.Trace("Registering server on Consul...");

        // TODO add configuration
        AgentServiceCheck healthCheck = new AgentServiceCheck()
        {
            HTTP = HealthCheckUrl,
            Interval = _healthCheckPollInterval,
            DeregisterCriticalServiceAfter = _healthCheckTimeout,
            Status = HealthStatus.Passing
        };
        
        // 나 

        AgentServiceRegistration service = new AgentServiceRegistration
        {
            ID = id.ToString(),
            Name = _serviceName,
            Address = host,
            Port = _serverPortInfo.serverPort,
            Tags = new string[] { "group:" + group },
            Meta = properties,
            Check = healthCheck
        };
        
        try
        {
            await _client.Agent.ServiceRegister(service);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to register server with Consul as an exception occurred.", e);
            throw e;
        }

        // Start timers and get an initial list
        FetchServices().Wait();
        CreateTimer(10000, 10000, (_) => FetchServices().Wait());
        
        return id;
    }

    private async Task<bool> InitKV()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Logger.Trace("Init the KV value, attempt: " + (attempt + 1));

            QueryResult<KVPair> result = await _client.KV.Get("darkrift-2/next-id");
            KVPair kvPair = result.Response;

            if (kvPair != null)
            {
                bool valid = ushort.TryParse(Encoding.UTF8.GetString(kvPair.Value, 0, kvPair.Value.Length),
                    out ushort id);
                if (!valid)
                    throw new InvalidOperationException(
                        "Failed to allocate ID as the stored next ID is not a valid ushort.");
                
                kvPair.Value = Encoding.UTF8.GetBytes((id).ToString());

                WriteResult<bool> casResult;
                try
                {
                    casResult = await _client.KV.CAS(kvPair);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to perform CAS operation on Consul while updating ID field.", e);
                    throw;
                }

                if (casResult.Response)
                    return true;
            }
            else
            {
                // First in the cluster, we need to create the next-id field!
                kvPair = new KVPair("darkrift-2/next-id")
                {
                    Value = Encoding.UTF8.GetBytes("1")
                };

                WriteResult<bool> casResult;
                try
                {
                    casResult = await _client.KV.CAS(kvPair);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to perform CAS operation on Consul while creating ID field.", e);
                    throw;
                }

                if (casResult.Response)
                    return true;
            }
        }

        return false;
    }
    /// <summary>
    ///     Allocates a new ID from Consul.
    /// </summary>
    /// <returns>A task object for the operation with the ID allocated.</returns>
    private async Task<ushort> AllocateID()
    {
        
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Logger.Trace("Allocating a new ID on Consul, attempt: " + (attempt + 1));

            QueryResult<KVPair> result = await _client.KV.Get("darkrift-2/next-id");
            KVPair kvPair = result.Response;
            List<ushort> allocatedIDs = new List<ushort>(0);
            if (kvPair != null)
            {
                bool valid = ushort.TryParse(Encoding.UTF8.GetString(kvPair.Value, 0, kvPair.Value.Length),
                    out ushort id);
                if (!valid)
                    throw new InvalidOperationException(
                        "Failed to allocate ID as the stored next ID is not a valid ushort.");
                
                //kvPair.Value = Encoding.UTF8.GetBytes((id + 1).ToString());
                Dictionary<string, AgentService>? resultResponse = _client.Agent.Services().Result.Response;
                
                foreach (KeyValuePair<string, AgentService> info in resultResponse)
                {
                    if (!ushort.TryParse(info.Value.ID, out ushort registedID)) continue;
                    if (registedID == id)
                        id++;

                    // id 범위는 10을 넘기지 말자.
                    if (registedID > 10)
                        id = 0;
                    
                    ServiceEntry[]? statuses = _client.Health.Service(_serviceName).Result.Response;
                    for (int i = 0; i < statuses.Length; i++)
                    {
                        /*
                            ClientServerPlugin.Debug($"{statuses[i].Node.Name}");
                            ClientServerPlugin.Debug($"{statuses[i].Service.Service}");
                            ClientServerPlugin.Debug($"{statuses[i].Service.ID}");
                            ClientServerPlugin.Debug($"{statuses[i].Service.Port}");
                            */
                        ServiceEntry current = statuses[i];
                        // 기존 id 저장.
                        allocatedIDs.Add(ushort.Parse(current.Service.ID));
                        // 상태 불량이 있으면 상태불량을 지운다.
                        if (current.Checks.Any(c => c.Status.Equals(HealthStatus.Critical)))
                        {
                            WriteResult? ret = _client.Agent.ServiceDeregister(current.Service.ID).Result;
                        }
                        // 중복 이 있으면 중복을 지운다.
                        else if (current.Service.Port == _serverPortInfo.serverPort)
                        {
                            WriteResult? ret = _client.Agent.ServiceDeregister(current.Service.ID).Result;
                        }
                        // 어떤 ID랑 지금 내 id 랑 겹칠거 같으면 내 id 를 변경한다.
                        else if (current.Service.ID == id.ToString())
                            id++;
                    }

                    // 어떤 이유든 10까지 번호가 꽉 차 있다면, 안겹칠때까지 다음 값으로 더해준다.
                    while (allocatedIDs.Contains(id))
                        id++;
                }
                kvPair.Value = Encoding.UTF8.GetBytes(id.ToString());

                WriteResult<bool> casResult;
                try
                {
                    casResult = await _client.KV.CAS(kvPair);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to perform CAS operation on Consul while updating ID field.", e);
                    throw;
                }
                
                if (casResult.Response)
                    return id;
            }
            else
            {
                // First in the cluster, we need to create the next-id field!
                kvPair = new KVPair("darkrift-2/next-id")
                {
                    Value = Encoding.UTF8.GetBytes("1")
                };

                WriteResult<bool> casResult;
                try
                {
                    casResult = await _client.KV.CAS(kvPair);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to perform CAS operation on Consul while creating ID field.", e);
                    throw;
                }

                if (casResult.Response)
                    return 0;
            }
        }

        Logger.Error(
            "Failed to allocate ID from Consul as the operation exceeded the maximum number of allowed attempts (10).");
        throw new InvalidOperationException(
            "Failed to allocate ID from Consul as the operation exceeded the maximum number of allowed attempts (10).");
    }

    /// <summary>
    ///     Disposes of the client.
    /// </summary>
    /// <param name="disposing">If we are disopsing.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _client.Dispose();
    }
}

