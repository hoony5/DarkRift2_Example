using DarkRift;
[System.Serializable]
public record Server : IDarkRiftSerializable
{
    public string ServerName { get; set; }
    public Avatar[] Characters { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        ServerName = e.Reader.ReadString();
        Characters = e.Reader.ReadSerializables<Avatar>();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ServerName);
        e.Writer.Write(Characters);
    }
}