using GrainLibrary.Grains;
using GrainLibrary.Models;
using GrainLibrary.Services;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp.Attendance;

namespace GameServer.Controllers;

public class AttendanceController(IClusterClient clusterClient)
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.LoadAttendance)]
    public async Task LoadAttendanceAsync(PlayerSession player, LoadAttendanceReq req)
    {
        var attendanceGrain = _clusterClient.GetGrain<IPlayerAttendanceGrain>(player.SessionId);

        var state = await attendanceGrain.GetAsync(req.EventId);

        await SendAsync(player, response: new LoadAttendanceRes
        {
            EventId = state.EventId,
            Day = state.Day,
            Claimed = state.Claimed,
            MaxDay = state.MaxDay,
        });
    }

    [PacketHandler(PacketHeaderType.CheckAttendance)]
    public async Task CheckAttendanceAsync(PlayerSession player, CheckAttendanceReq req)
    {
        var attendanceGrain = _clusterClient.GetGrain<IPlayerAttendanceGrain>(player.SessionId);

        var result = await attendanceGrain.CheckAsync(req.EventId);
        if (result.ResultCode != ResultCode.Success)
        {
            await SendAsync<CheckAttendanceRes>(player, result.ResultCode);
            return;
        }

        await SendAsync(player, response: new CheckAttendanceRes
        {
            EventId = result.EventId,
            Day = result.Day,
            Claimed = result.Claimed,
        });
    }

    [PacketHandler(PacketHeaderType.ClaimAttendance)]
    public async Task ClaimAttendanceAsync(PlayerSession player, ClaimAttendanceReq req)
    {
        var attendanceGrain = _clusterClient.GetGrain<IPlayerAttendanceGrain>(player.SessionId);

        var result = await attendanceGrain.ClaimAsync(req.EventId);
        if (result.ResultCode != ResultCode.Success)
        {
            await SendAsync<ClaimAttendanceRes>(player, result.ResultCode);
            return;
        }

        await SendAsync(player, response: new ClaimAttendanceRes
        {
            EventId = result.EventId,
            Day = result.Day,
            RewardCurrencyType = result.RewardCurrencyType,
            RewardCurrencyAmount = result.RewardCurrencyAmount,
            RewardItemId = result.RewardItemId,
            RewardItemCount = result.RewardItemCount,
            WalletInfo = result.WalletInfo,
        });
    }
}
