using ScenarioBot.Scenario;
using ScenarioBot.Session;

public class Program
{
    public static async Task Main()
    {
        const string host     = "127.0.0.1";
        const int    port     = 35000;
        const int    botCount = 100;
 
        var scenario = new LoadPlayerScenario();
 
        Console.WriteLine($"[Bot] {botCount}개 봇으로 '{scenario.Name}' 시나리오 시작");
        Console.WriteLine($"[Bot] 대상: {host}:{port}");
        Console.WriteLine("──────────────────────────────");
 
        var manager = new BotManager(host, port, scenario, botCount);
        var results = await manager.RunAsync();
    }
}