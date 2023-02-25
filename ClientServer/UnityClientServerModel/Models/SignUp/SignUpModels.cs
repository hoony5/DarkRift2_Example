using DarkRift;

[System.Serializable]
public record SignUpRequest : IDarkRiftSerializable
{
    public ushort ClientID { get; set; }
    public string AccountID { get; set; }
    public string Password { get; set; } 
    public string NickName { get; set; }
    
    public void Deserialize(DeserializeEvent e)
    {
        ClientID = e.Reader.ReadUInt16();
        AccountID = e.Reader.ReadString();
        Password = e.Reader.ReadString();
        NickName = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ClientID);
        e.Writer.Write(AccountID);
        e.Writer.Write(Password);
        e.Writer.Write(NickName);
    }
}
public record SignUpSuccess : IDarkRiftSerializable
{
    public ushort ClientID { get; set; } 
    // TODO :: will be Changed User Info
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
public record SignUpFailed : IDarkRiftSerializable
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