using DarkRift;

public record SignInRequest : IDarkRiftSerializable
{
    public ushort ClientID { get; set; } 
    public string AccountID { get; set; }
    public string Password { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        ClientID = e.Reader.ReadUInt16();
        AccountID = e.Reader.ReadString();
        Password = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ClientID);
        e.Writer.Write(AccountID);
        e.Writer.Write(Password);
    }
}
public record SignInSuccess : IDarkRiftSerializable
{
    public ushort ClientID { get; set; } 
    
    public string PersonalKey { get; set; }
    public User User { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        ClientID = e.Reader.ReadUInt16();
        PersonalKey = e.Reader.ReadString();
        User = e.Reader.ReadSerializable<User>();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ClientID);
        e.Writer.Write(PersonalKey);
        e.Writer.Write(User);
    }
}
public record SignInFailed : IDarkRiftSerializable
{
    public ushort ClientID { get; set; } 
    public bool AccountValidation { get; set; } 
    public bool PasswordValidation { get; set; }

    public void Deserialize(DeserializeEvent e)
    {
        ClientID = e.Reader.ReadUInt16();
        AccountValidation = e.Reader.ReadBoolean();
        PasswordValidation = e.Reader.ReadBoolean();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ClientID);
        e.Writer.Write(AccountValidation);
        e.Writer.Write(PasswordValidation);
    }
}