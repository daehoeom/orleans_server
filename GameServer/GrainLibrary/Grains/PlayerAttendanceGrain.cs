using Database.Db;
using Database.Db.Row;
using GrainLibrary.Grains.Dto;
using GrainLibrary.Resource;
using GrainLibrary.Utility;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerAttendanceGrain : IGrainWithIntegerKey
{
    Task<AttendanceStateDto>? GetAsync(int eventId);
    Task<AttendanceRewardResultDto> ReceiveRewardAsync(int eventId, int day);
}

public class PlayerAttendanceGrain(DatabaseService dbService, ResourceService resourceService) : Grain, IPlayerAttendanceGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    private readonly Dictionary<int, AttendanceStateDto> _progress = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var progressRows = await dbService.Game.EventAttendance.GetsAsync(PlayerId);
        foreach (var row in progressRows)
        {
            _progress[row.event_id] = new AttendanceStateDto
            {
                EventId = row.event_id,
                CurrentDay = row.day,
            };
        }

        var rewardRows = await dbService.Game.EventAttendanceRewards.GetsAsync(PlayerId);
        foreach (var row in rewardRows)
        {
            if (!_progress.TryGetValue(row.event_id, out var dto))
            {
                continue;
            }

            dto.RewardReceivedFlag.Add(row.reward_id, row.received_flag);
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<AttendanceStateDto>? GetAsync(int eventId)
    {
        return !_progress.TryGetValue(eventId, out var progressValue) 
            ? null 
            : Task.FromResult(progressValue);
    }

    public async Task<AttendanceRewardResultDto> ReceiveRewardAsync(int eventId, int day)
    {
        var rAttendance = resourceService.Attendance.Get(eventId, day);
        if (rAttendance is null)
        {
            return new AttendanceRewardResultDto { ResultCode = ResultCode.AttendanceEventNotFound };
        }

        if (!_progress.TryGetValue(eventId, out var eventData))
        {
            return new AttendanceRewardResultDto() { ResultCode = ResultCode.NotCheckedYet };
        }

        if (eventData.RewardReceivedFlag.TryGetValue(rAttendance.RewardId, out var flag))
        {
            if (flag)
            {
                return new AttendanceRewardResultDto { ResultCode = ResultCode.AlreadyRewardReceived };
            }
        }
        
        var insertedRow = await dbService.Game.EventAttendanceRewards.InsertAsync(new PlayerEventAttendanceRewardRow
        {
            player_id = PlayerId,
            event_id = rAttendance.EventId,
            reward_id = rAttendance.RewardId,
            received_flag = true,
        });
        if (insertedRow <= 0)
        {
            return new AttendanceRewardResultDto { ResultCode = ResultCode.DbInsertError };
        }

        var walletGrain = GrainFactory.GetGrain<IPlayerWalletGrain>(PlayerId);

        var rewardGrant = await RewardHelper.GrantAsync(
            GrainFactory, PlayerId,
            [(rAttendance.RewardCurrencyType, rAttendance.RewardCurrencyAmount)],
            [(rAttendance.RewardItemId, rAttendance.RewardItemCount)]);

        var walletInfo = await walletGrain.GetAllBalanceAsync(); 
        
        return new AttendanceRewardResultDto
        {
            ResultCode = ResultCode.Success,
            EventId = eventId,
            Day = day,
            RewardCurrencyType = rAttendance.RewardCurrencyType,
            RewardCurrencyAmount = rAttendance.RewardCurrencyAmount,
            RewardItemId = rAttendance.RewardItemId,
            RewardItemCount = rAttendance.RewardItemCount,
            WalletInfo = walletInfo,
            RewardGrant = rewardGrant,
        };
    }
}
