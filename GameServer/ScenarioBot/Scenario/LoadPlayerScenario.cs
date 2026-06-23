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
        await session.SendAsync(PacketHeaderType.KeepAlive, new KeepAliveReq
        {

        });

        var keepAliveRes = await session.WaitForResponseAsync<KeepAliveRes>();
        if (keepAliveRes.ResultCode != ResultCode.Success)
        {
            throw new Exception($"[{Name}] 실패: {keepAliveRes.ResultCode}");
        }
        
        // 2. 정보 로드
        await session.SendAsync(PacketHeaderType.LoadPlayer, new LoadPlayerReq
        {
        });

        var loadRes = await session.WaitForResponseAsync<LoadPlayerRes>();
        if (loadRes.ResultCode != ResultCode.Success)
        {
            throw new Exception($"[{Name}] 실패: {loadRes.ResultCode}");
        }
    }
}