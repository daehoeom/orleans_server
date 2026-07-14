using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class RUnitDataSet : IDataSet<RUnit>
{
    private readonly Dictionary<int, RUnit> _units = new();

    public bool Load(RUnit data)
    {
        return _units.TryAdd(data.Id, data);
    }

    public bool PostProcess(RUnit data)
    {
        return data.Id > 0;
    }

    public RUnit? Find(int id)
    {
        return _units.GetValueOrDefault(id);
    }
}
