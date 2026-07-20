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

        Console.WriteLine($"[{Name}] 현재 상태: Day={loadRes.Stream.Day}/{loadRes.Stream.MaxDay}, Claimed={loadRes.Stream.Claimed}");

        await session.SendAsync(PacketHeaderType.CheckAttendance, new CheckAttendanceReq
        {
            EventId = EventId,
        });

        var checkRes = await session.WaitForResponseAsync<CheckAttendanceRes>();
        if (checkRes.ResultCode != ResultCode.Success && checkRes.ResultCode != ResultCode.AlreadyCheckedToday)
        {
            throw new Exception($"[{Name}] CheckAttendance 실패: {checkRes.ResultCode}");
        }

        Console.WriteLine($"[{Name}] 출석 체크: Day={checkRes.Stream.Day}, ResultCode={checkRes.ResultCode}");

        await session.SendAsync(PacketHeaderType.ClaimAttendance, new ClaimAttendanceReq
        {
            EventId = EventId,
        });

        var claimRes = await session.WaitForResponseAsync<ClaimAttendanceRes>();
        if (claimRes.ResultCode != ResultCode.Success && claimRes.ResultCode != ResultCode.AlreadyClaimed)
        {
            throw new Exception($"[{Name}] ClaimAttendance 실패: {claimRes.ResultCode}");
        }

        Console.WriteLine($"[{Name}] 보상 수령: Day={claimRes.Stream.Day}, ResultCode={claimRes.ResultCode}");
    }
}
