namespace Observer.Shared.DTOs.Hunters;

public sealed class HunterHealthDto
{
    public string ServerId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public string Kind { get; set; } = "";

    public bool IsRunning { get; set; }
    public int WorkCapacity { get; set; }
    public int CurrentWorkLoad { get; set; }

    // Kept for client compatibility — mirrors IsRunning
    public bool IsHealthy => IsRunning;

    public DateTime? LastCheckedUtc { get; set; }
    public int? LatencyMs { get; set; }
    public string? Error { get; set; }
}
