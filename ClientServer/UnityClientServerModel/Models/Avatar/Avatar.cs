using DarkRift;
[System.Serializable]
public record Avatar : IDarkRiftSerializable
{
    public string Name { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        Name = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Name);
    }
}