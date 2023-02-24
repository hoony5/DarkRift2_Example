namespace UnityClientServer;

public record ServerPortInfo(ushort healthCheckPort, ushort serverPort)
{
    public ushort HealthCheckPort { get; init; } = healthCheckPort;
    public ushort Port { get; init; } = serverPort;
}