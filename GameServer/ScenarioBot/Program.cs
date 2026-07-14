using ScenarioBot.Scenario;
using ScenarioBot.Session;

public class Program
{
    private static readonly Dictionary<string, IScenario> Scenarios = new()
    {
        ["LoadPlayer"] = new LoadPlayerScenario(),
        ["ShopPurchase"] = new ShopPurchaseScenario(),
    };

    public static async Task Main(string[] args)
    {
        const string host     = "127.0.0.1";
        const int    port     = 35000;
        const int    botCount = 1;

        var scenarioName = args.Length > 0 ? args[0] : "LoadPlayer";
        if (!Scenarios.TryGetValue(scenarioName, out var scenario))
        {
            throw new ArgumentException($"알 수 없는 시나리오: {scenarioName} (사용 가능: {string.Join(", ", Scenarios.Keys)})");
        }

        Console.WriteLine($"[Bot] {botCount}개 봇으로 '{scenario.Name}' 시나리오 시작");
        Console.WriteLine($"[Bot] 대상: {host}:{port}");
        Console.WriteLine("──────────────────────────────");

        _ = Console.ReadLine();
 
        var manager = new BotManager(host, port, scenario, botCount);
        var results = await manager.RunAsync();
    }
}