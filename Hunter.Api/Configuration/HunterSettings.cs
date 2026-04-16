namespace Hunter.Api.Configuration;

public class HunterSettings
{
    public string Role { get; set; } = "QueenBee";

    public bool IsQueenBee => Role == "QueenBee";
    public bool IsWorkerBee => Role == "WorkerBee";
}
