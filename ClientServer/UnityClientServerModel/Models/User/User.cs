using DarkRift;

[System.Serializable]
public record User : IDarkRiftSerializable
{
    public string AccountID { get; set; }
    public string NickName { get; set; }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(AccountID);
        e.Writer.Write(NickName);
    }
    public void Deserialize(DeserializeEvent e)
    {
        AccountID = e.Reader.ReadString();
        NickName = e.Reader.ReadString();
    }
}