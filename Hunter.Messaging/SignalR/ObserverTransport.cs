namespace SignalR;

public class ObserverTransport
{
    public Guid     Id            { get; set; } = Guid.NewGuid(); // For tracking
    public string   EngineerId    { get; set; }     // Engineer who owns the problem
    public string   ProblemTitle  { get; set; }     // Problem being investigated 
    
    // Initially Parameters will be set to LogMessageParms
    public object   Parameters    { get; set; }     // based on the Observer request/response type
}
