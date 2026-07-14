using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class RUnitLevelDataSet : IDataSet<RUnitLevel>
{
    private readonly Dictionary<(int UnitId, int Level), RUnitLevel> _levels = new();

    public bool Load(RUnitLevel data)
    {
        return _levels.TryAdd((data.UnitId, data.Level), data);
    }

    public bool PostProcess(RUnitLevel data)
    {
        return data is { UnitId: > 0, Level: > 0 };
    }

    public RUnitLevel? Find(int unitId, int level)
    {
        return _levels.GetValueOrDefault((unitId, level));
    }
}
