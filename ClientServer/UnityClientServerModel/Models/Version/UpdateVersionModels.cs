using DarkRift;

public record UpdateVersionRequest : IDarkRiftSerializable
{
    public ushort Current { get; set; }
    public ushort ClientID { get; set; }
    public string URL { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        Current = e.Reader.ReadUInt16();
        ClientID = e.Reader.ReadUInt16();
        URL = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Current);
        e.Writer.Write(ClientID);
        e.Writer.Write(URL);
    }
}
public record struct UpdateVersionSuccess : IDarkRiftSerializable
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
public record struct UpdateVersionFailed : IDarkRiftSerializable
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