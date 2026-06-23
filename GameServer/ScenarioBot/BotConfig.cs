namespace ScenarioBot;

public record BotConfig
{
    public string Host { get; init; } = "127.0.0.1";

    public int Port { get; init; } = 35000;

    public int BotCount { get; init; } = 100;

    public string Scenario { get; init; } = string.Empty;
}