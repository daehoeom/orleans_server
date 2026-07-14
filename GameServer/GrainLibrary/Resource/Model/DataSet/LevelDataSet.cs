using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class LevelDataSet : IDataSet<RLevel>
{
    private readonly Dictionary<int, RLevel> _levels = new();

    public bool Load(RLevel data)
    {
        return _levels.TryAdd(data.Level, data);
    }

    public bool PostProcess(RLevel data)
    {
        return data is { Level: > 0, RequiredExp: > 0 };
    }

    public RLevel? Find(int level)
    {
        return _levels.GetValueOrDefault(level);
    }
}
