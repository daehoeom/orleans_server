using ScenarioBot.Session;
using SharedLibrary;
using SharedLibrary.Packet.Tcp.Attendance;

namespace ScenarioBot.Scenario;

public class AttendanceScenario : IScenario
{
    private const int EventId = 1;

    public string Name => "Attendance";

    public async Task RunAsync(BotClientSession session)
    {
        await session.SendAsync(PacketHeaderType.LoadAttendance, new LoadAttendanceReq
        {
            EventId = EventId,
        });

        var loadRes = await session.WaitForResponseAsync<LoadAttendanceRes>();
        if (loadRes.ResultCode != ResultCode.Success)
        {
            throw new Exception($"[{Name}] LoadAttendance 실패: {loadRes.ResultCode}");
        }

        await session.SendAsync(PacketHeaderType.ReceiveAttendanceReward, new ReceiveAttendanceRewardReq
        {
            EventId = EventId,
            Day = 1,
        });

        var claimRes = await session.WaitForResponseAsync<ReceiveAttendanceRewardRes>();
        if (claimRes.ResultCode != ResultCode.Success && claimRes.ResultCode != ResultCode.AlreadyRewardReceived)
        {
            throw new Exception($"[{Name}] ClaimAttendance 실패: {claimRes.ResultCode}");
        }

        Console.WriteLine($"[{Name}] 보상 수령: Day={claimRes.Stream.Day}, ResultCode={claimRes.ResultCode}");
    }
}
