namespace SigPipes;

public class SigPipeRequest
{
    public string Payload { get; set; }

    // Optional: Add transport-agnostic metadata
    public string Channel { get; set; } = "SignalR";  // or "NamedPipe"
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
