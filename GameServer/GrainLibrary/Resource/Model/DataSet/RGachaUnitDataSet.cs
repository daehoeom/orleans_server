using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class RGachaUnitDataSet : IDataSet<RGachaUnit>
{
    private readonly Dictionary<int, List<RGachaUnit>> _pools = new();

    public bool Load(RGachaUnit data)
    {
        if (!_pools.TryGetValue(data.GachaId, out var pool))
        {
            pool = [];
            _pools[data.GachaId] = pool;
        }

        pool.Add(data);

        return true;
    }

    public bool PostProcess(RGachaUnit data)
    {
        return data is { GachaId: > 0, UnitId: > 0, Weight: > 0 };
    }

    public IReadOnlyList<RGachaUnit> FindAll(int gachaId)
    {
        return _pools.GetValueOrDefault(gachaId) ?? [];
    }
}
