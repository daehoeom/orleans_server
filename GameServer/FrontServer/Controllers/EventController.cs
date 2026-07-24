using GrainLibrary.Grains;
using GrainLibrary.Models;
using GrainLibrary.Resource;
using GrainLibrary.Services;
using GrainLibrary.Utility;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp.Attendance;

namespace GameServer.Controllers;

public class EventController(IClusterClient clusterClient, ResourceService resourceService)
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.LoadAttendance)]
    public async Task LoadAttendanceAsync(PlayerSession player, LoadAttendanceReq req)
    {
        var attendanceGrain = _clusterClient.GetGrain<IPlayerAttendanceGrain>(player.SessionId);

        var rEventSchedule = resourceService.EventSchedule.Get(req.EventId);
        if (rEventSchedule is null)
        {
            await SendAsync<LoadAttendanceRes>(player, ResultCode.AttendanceEventNotFound);
            return;
        }

        if (TimeUtil.IsExpired(rEventSchedule.EndDateTime))
        {
            await SendAsync<LoadAttendanceRes>(player, ResultCode.EventEnded);
            return;
        }
        
        var state = await attendanceGrain.GetAsync(req.EventId);
        await SendAsync(player, response: new LoadAttendanceRes
        {
            EventId = state.EventId,
            Day = state.CurrentDay,
            ReceiveRewardFlag = state.RewardReceivedFlag,
        });
    }

    [PacketHandler(PacketHeaderType.ReceiveAttendanceReward)]
    public async Task ReceiveAttendanceRewardAsync(PlayerSession player, ReceiveAttendanceRewardReq req)
    {
        var attendanceGrain = _clusterClient.GetGrain<IPlayerAttendanceGrain>(player.SessionId);

        var result = await attendanceGrain.ReceiveRewardAsync(req.EventId, req.Day);
        if (result.ResultCode != ResultCode.Success)
        {
            await SendAsync<ReceiveAttendanceRewardRes>(player, result.ResultCode);
            return;
        }

        await SendAsync(player, response: new ReceiveAttendanceRewardRes
        {
            EventId = result.EventId,
            Day = result.Day,
            RewardResult = result.RewardResult,
        });
    }
}
