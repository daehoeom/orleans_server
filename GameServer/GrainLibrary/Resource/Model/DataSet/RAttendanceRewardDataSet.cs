using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class RAttendanceRewardDataSet : IDataSet<RAttendanceReward>
{
    private readonly Dictionary<int, RAttendanceReward> _rewardsById = new();
    private readonly Dictionary<(int EventId, int Day), RAttendanceReward> _rewardsByEventDay = new();
    private readonly Dictionary<int, int> _maxDayByEvent = new();

    public bool Load(RAttendanceReward data)
    {
        if (!_rewardsById.TryAdd(data.RewardId, data))
        {
            return false;
        }

        return _rewardsByEventDay.TryAdd((data.EventId, data.Day), data);
    }

    public bool PostProcess(RAttendanceReward data)
    {
        if (data is not { RewardId: > 0, EventId: > 0, Day: > 0 })
        {
            return false;
        }

        _maxDayByEvent[data.EventId] = Math.Max(_maxDayByEvent.GetValueOrDefault(data.EventId), data.Day);

        return true;
    }

    public RAttendanceReward? Find(int eventId, int day) => _rewardsByEventDay.GetValueOrDefault((eventId, day));

    public RAttendanceReward? FindById(int rewardId) => _rewardsById.GetValueOrDefault(rewardId);

    public int GetMaxDay(int eventId) => _maxDayByEvent.GetValueOrDefault(eventId);
}
