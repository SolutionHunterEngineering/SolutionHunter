namespace Observer.Shared.DTOs.Hunters;

public sealed class HunterHealthDto
{
    public string ServerId { get; set; } = "";
    public string Name { get; set; } = "";
    public string BaseUrl { get; set; } = "";

    public bool IsHealthy { get; set; }
    public DateTime? LastCheckedUtc { get; set; }
    public int? LatencyMs { get; set; }
    public string? Error { get; set; }
}
