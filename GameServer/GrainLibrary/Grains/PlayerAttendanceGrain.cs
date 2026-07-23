using Database.Db;
using Database.Db.Row;
using GrainLibrary.Grains.Dto;
using GrainLibrary.Resource;
using GrainLibrary.Utility;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerAttendanceGrain : IGrainWithIntegerKey
{
    Task<AttendanceStateDto> GetAsync(int eventId);
    Task<AttendanceCheckResultDto> CheckAsync(int eventId);
    Task<AttendanceClaimResultDto> ClaimAsync(int eventId);
}

public class PlayerAttendanceGrain(DatabaseService dbService, ResourceLoader resourceLoader) : Grain, IPlayerAttendanceGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    private class EventProgress
    {
        public int Day { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    private readonly Dictionary<int, EventProgress> _progress = new();
    private readonly HashSet<int> _claimedRewardIds = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var progressRows = await dbService.Game.EventAttendance.GetsAsync(PlayerId);
        foreach (var row in progressRows)
        {
            _progress[row.event_id] = new EventProgress
            {
                Day = row.day,
                LastUpdatedAt = row.last_updated_at,
            };
        }

        var rewardRows = await dbService.Game.EventAttendanceRewards.GetsAsync(PlayerId);
        foreach (var row in rewardRows)
        {
            if (row.received_flag)
            {
                _claimedRewardIds.Add(row.reward_id);
            }
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<AttendanceStateDto> GetAsync(int eventId)
    {
        var day = _progress.GetValueOrDefault(eventId)?.Day ?? 0;
        var reward = day > 0 ? resourceLoader.AttendanceReward.Find(eventId, day) : null;
        var claimed = reward is not null && _claimedRewardIds.Contains(reward.RewardId);

        return Task.FromResult(new AttendanceStateDto
        {
            EventId = eventId,
            Day = day,
            Claimed = claimed,
            MaxDay = resourceLoader.AttendanceReward.GetMaxDay(eventId),
        });
    }

    public async Task<AttendanceCheckResultDto> CheckAsync(int eventId)
    {
        var maxDay = resourceLoader.AttendanceReward.GetMaxDay(eventId);
        if (maxDay <= 0)
        {
            return new AttendanceCheckResultDto { ResultCode = ResultCode.AttendanceEventNotFound };
        }

        _progress.TryGetValue(eventId, out var progress);
        var currentDay = progress?.Day ?? 0;
        if (currentDay >= maxDay)
        {
            return new AttendanceCheckResultDto { ResultCode = ResultCode.AttendanceEventEnded };
        }

        var today = TimeUtil.UtcNow.Date;
        if (progress is not null && progress.LastUpdatedAt.Date == today)
        {
            return new AttendanceCheckResultDto { ResultCode = ResultCode.AlreadyCheckedToday };
        }

        var newDay = currentDay + 1;

        if (progress is null)
        {
            var insertedRow = await dbService.Game.EventAttendance.InsertAsync(new PlayerEventAttendanceRow
            {
                player_id = PlayerId,
                event_id = eventId,
                day = newDay,
            });
            if (insertedRow <= 0)
            {
                return new AttendanceCheckResultDto { ResultCode = ResultCode.DbInsertError };
            }

            _progress[eventId] = new EventProgress { Day = newDay, LastUpdatedAt = today };
        }
        else
        {
            var affectedRow = await dbService.Game.EventAttendance.UpdateAsync(PlayerId, eventId, newDay, today);
            if (affectedRow <= 0)
            {
                return new AttendanceCheckResultDto { ResultCode = ResultCode.DbUpdateError };
            }

            progress.Day = newDay;
            progress.LastUpdatedAt = today;
        }

        return new AttendanceCheckResultDto
        {
            ResultCode = ResultCode.Success,
            EventId = eventId,
            Day = newDay,
            Claimed = false,
        };
    }

    public async Task<AttendanceClaimResultDto> ClaimAsync(int eventId)
    {
        var day = _progress.GetValueOrDefault(eventId)?.Day ?? 0;
        if (day <= 0)
        {
            return new AttendanceClaimResultDto { ResultCode = ResultCode.NotCheckedYet };
        }

        var reward = resourceLoader.AttendanceReward.Find(eventId, day);
        if (reward is null)
        {
            return new AttendanceClaimResultDto { ResultCode = ResultCode.AttendanceRewardNotFound };
        }

        if (_claimedRewardIds.Contains(reward.RewardId))
        {
            return new AttendanceClaimResultDto { ResultCode = ResultCode.AlreadyClaimed };
        }

        var insertedRow = await dbService.Game.EventAttendanceRewards.InsertAsync(new PlayerEventAttendanceRewardRow
        {
            player_id = PlayerId,
            event_id = eventId,
            reward_id = reward.RewardId,
            received_flag = true,
        });
        if (insertedRow <= 0)
        {
            return new AttendanceClaimResultDto { ResultCode = ResultCode.DbInsertError };
        }

        _claimedRewardIds.Add(reward.RewardId);

        var walletGrain = GrainFactory.GetGrain<IPlayerWalletGrain>(PlayerId);

        var rewardGrant = await RewardHelper.GrantAsync(
            GrainFactory, PlayerId,
            [(reward.RewardCurrencyType, reward.RewardCurrencyAmount)],
            [(reward.RewardItemId, reward.RewardItemCount)]);

        return new AttendanceClaimResultDto
        {
            ResultCode = ResultCode.Success,
            EventId = eventId,
            Day = day,
            RewardCurrencyType = reward.RewardCurrencyType,
            RewardCurrencyAmount = reward.RewardCurrencyAmount,
            RewardItemId = reward.RewardItemId,
            RewardItemCount = reward.RewardItemCount,
            WalletInfo = await walletGrain.GetAllBalanceAsync(),
            RewardGrant = rewardGrant,
        };
    }
}
