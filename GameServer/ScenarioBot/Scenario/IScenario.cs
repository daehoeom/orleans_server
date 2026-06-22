using ScenarioBot.Session;

namespace ScenarioBot.Scenario;

public interface IScenario
{
    string Name { get; }
    Task RunAsync(BotClientSession session);
}