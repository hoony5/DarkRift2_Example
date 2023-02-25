using DarkRift;

[System.Serializable]
public record SignOutRequest : IDarkRiftSerializable
{
    public ushort ClientID { get; set; }
    public string AccountID { get; set; } 

    public void Deserialize(DeserializeEvent e)
    {
        ClientID = e.Reader.ReadUInt16();
        AccountID = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ClientID);
        e.Writer.Write(AccountID);
    }
}
public record SignOutSuccess : IDarkRiftSerializable
{
    public ushort ClientID { get; set; }
    public string AccountID { get; set; }
    // TODO :: will be Changed User Info

    public void Deserialize(DeserializeEvent e)
    {
        ClientID = e.Reader.ReadUInt16();
        AccountID = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ClientID);
        e.Writer.Write(AccountID);
    }
}
public record SignOutFailed : IDarkRiftSerializable
{
    public ushort ClientID { get; set; }
    public string Log { get; set; }

    public void Deserialize(DeserializeEvent e)
    {
        ClientID = e.Reader.ReadUInt16();
        Log = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ClientID);
        e.Writer.Write(Log);
    }
}