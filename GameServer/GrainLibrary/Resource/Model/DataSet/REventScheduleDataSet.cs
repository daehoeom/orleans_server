using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class REventScheduleDataSet : IDataSet<REventSchedule>
{
    private readonly Dictionary<int, REventSchedule> _schedules = new();
    
    public bool Load(REventSchedule data)
    {
        if (!_schedules.TryAdd(data.EventId, data))
        {
            return false;
        }

        return true;
    }

    public bool PostProcess(REventSchedule data)
    {
        return true;
    }

    public REventSchedule? Get(int eventId)
        => _schedules.GetValueOrDefault(eventId);
}