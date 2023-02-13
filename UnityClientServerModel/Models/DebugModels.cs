namespace UnityClientServerModel.Models;
using DarkRift;

public record DebugModelRequest : IDarkRiftSerializable
{
    public string ServerName { get; set; }
    public Tags Tags { get; set; }
    public string Log { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        Tags = (Tags)e.Reader.ReadUInt16();
        ServerName = e.Reader.ReadString();
        Log = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        if (string.IsNullOrEmpty(ServerName))
        {
            Console.WriteLine($"{Tags} | ServerName is Null)");
            return;
        }
        e.Writer.Write(ServerName);
        
        if (string.IsNullOrEmpty(Log))
        {
            Console.WriteLine($"{Tags} | Data is Null)");
            return;
        }
        e.Writer.Write(Log);
        e.Writer.Write((ushort)Tags);
    }
}
public record DebugModelResult : IDarkRiftSerializable
{
    public Tags Tags { get; set; }
    public string Result { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        Tags = (Tags)e.Reader.ReadUInt16();
        Result = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write((ushort)Tags);

        if (string.IsNullOrEmpty(Result))
        {
            Console.WriteLine($"{Tags} | Data is Null)");
            return;
        }
        e.Writer.Write(Result);
    }
}