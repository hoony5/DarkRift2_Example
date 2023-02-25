using DarkRift;
[System.Serializable]
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
[System.Serializable]
public record GetServerListSuccess : IDarkRiftSerializable
{
    // TODO :: Will be Changed to Data Structure
    public string[] ServerAllNames { get; set; }
    public Server[] Servers { get; set; }
    public ushort ClientID { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        ServerAllNames = e.Reader.ReadStrings();
        Servers = e.Reader.ReadSerializables<Server>();
        ClientID = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ServerAllNames);
        e.Writer.Write(Servers);
        e.Writer.Write(ClientID);
    }
}
[System.Serializable]
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