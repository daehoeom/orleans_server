using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class RItemDataSet : IDataSet<RItem>
{
    private readonly Dictionary<int, RItem> _items = new();

    public bool Load(RItem data)
    {
        return _items.TryAdd(data.Id, data);
    }

    public bool PostProcess(RItem data)
    {
        return true;
    }

    public RItem? Find(int productId)
    {
        return _items.GetValueOrDefault(productId);
    }
}