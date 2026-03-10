namespace Observer.Auth;

public class Server
{
    public int    ServerId { get; set; }
    public string Name     { get; set; } = "";  // "Observer", "Hunter-A", etc.
    public string Kind     { get; set; } = "";  // optional: same as Name or a type
}
