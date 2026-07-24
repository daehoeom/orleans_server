using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class RAttendanceDataSet : IDataSet<RAttendanceReward>
{
    private readonly Dictionary<int, RAttendanceReward> _rewardsById = new();
    private readonly Dictionary<(int EventId, int Day), RAttendanceReward> _rewardsByEventDay = new();
    private readonly Dictionary<int, List<RAttendanceReward>> _rewardsByEventId = new();
    private readonly Dictionary<int, int> _maxDayByEvent = new();

    public bool Load(RAttendanceReward data)
    {
        if (!_rewardsById.TryAdd(data.RewardId, data))
        {
            return false;
        }

        if (!_rewardsByEventDay.TryAdd((data.EventId, data.Day), data))
        {
            return false;
        }

        if (!_rewardsByEventId.TryGetValue(data.EventId, out var container))
        {
            container = [];
            _rewardsByEventId.Add(data.EventId, container);
        }

        container.Add(data);
        return true;
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

    public RAttendanceReward? Get(int eventId, int day) => _rewardsByEventDay.GetValueOrDefault((eventId, day));

    public RAttendanceReward? Get(int rewardId) => _rewardsById.GetValueOrDefault(rewardId);

    public List<RAttendanceReward> GetGroup(int eventId) => 
        _rewardsByEventId.GetValueOrDefault(eventId, []);

    public int GetMaxDay(int eventId) => _maxDayByEvent.GetValueOrDefault(eventId);
}
