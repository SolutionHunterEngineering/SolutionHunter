namespace Observer.Shared.DTOs;

public class ProblemCase
{
    public Guid Id { get; set; }

    /// The (Engineer, HunterServer) workspace this case belongs to.
    public Guid ServerUserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime CreatedUtc { get; set; }
    public DateTime? ClosedUtc { get; set; }
}
