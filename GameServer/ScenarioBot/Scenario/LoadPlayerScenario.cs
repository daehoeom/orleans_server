using ScenarioBot.Session;
using SharedLibrary;
using SharedLibrary.Packet.Tcp;

namespace ScenarioBot.Scenario;

public class LoadPlayerScenario : IScenario
{
    public string Name => "LoadPlayer";

    public async Task RunAsync(BotClientSession session)
    {
        //1. 요청 송신
        await session.SendAsync(PacketHeaderType.LoadPlayer, new LoadPlayerReq()
        {

        });

        var response = await session.WaitForResponseAsync<LoadPlayerRes>();
        if (response.ResultCode != ResultCode.Success)
        {
            throw new Exception($"[{Name}] 실패: {response.ResultCode}");
        }
    }
}