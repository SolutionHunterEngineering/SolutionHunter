namespace Hunter.Auth;

public class Server
{
    public int    ServerId { get; set; }
    public string Name     { get; set; } = "";  // "Hunter", "Hunter-A", etc.
    public string Kind     { get; set; } = "";  // optional: same as Name or a type
}
