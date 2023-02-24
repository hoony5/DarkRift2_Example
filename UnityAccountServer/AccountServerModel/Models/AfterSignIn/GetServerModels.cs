using DarkRift;
public record GetServerListRequest : IDarkRiftSerializable
{
    public string AccountID { get; set; }
    public ushort ClientID { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        AccountID = e.Reader.ReadString();
        ClientID = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(AccountID);
        e.Writer.Write(ClientID);
    }
}
public record GetServerListSuccess : IDarkRiftSerializable
{
    // TODO :: Will be Changed to Data Structure
    public string[] ServerAllNames { get; set; }
    public int[] YourServers { get; set; }
    public ushort ServerStatus { get; set; }
    public ushort ClientID { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        ServerAllNames = e.Reader.ReadStrings();
        YourServers = e.Reader.ReadInt32s();
        ClientID = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ServerAllNames);
        e.Writer.Write(YourServers);
        e.Writer.Write(ClientID);
    }
}
public record struct GetServerListFailed : IDarkRiftSerializable
{
    public ushort Current { get; set; } 
    public ushort ClientID { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        Current = e.Reader.ReadUInt16();
        ClientID = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Current);
        e.Writer.Write(ClientID);
    }
}