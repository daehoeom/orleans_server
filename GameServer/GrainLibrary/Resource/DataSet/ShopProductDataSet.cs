using GrainLibrary.Resource.Model;
using GrainLibrary.Resource.Model.Row;
using SharedLibrary;

namespace GrainLibrary.Resource.DataSet;

public class ShopProductDataSet : IDataSet<ShopProductRow>
{
    private readonly Dictionary<int, ShopProductRow> _products = new();

    public bool Load(ShopProductRow data)
    {
        return _products.TryAdd(data.ProductId, data);
    }

    public bool PostProcess(ShopProductRow data)
    {
        return data.Currency != CurrencyType.None && data.Price > 0;
    }

    public ShopProductRow? Find(int productId)
    {
        return _products.GetValueOrDefault(productId);
    }
}
