namespace SigPipes;

public class SigPipeResponse
{
    public Guid RequestID { get; set; }
    public object Result { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
