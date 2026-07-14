using GrainLibrary.Resource.Model.Row;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.DataSet;

public class RShopProductDataSet : IDataSet<RShopProduct>
{
    private readonly Dictionary<int, RShopProduct> _products = new();

    public bool Load(RShopProduct data)
    {
        return _products.TryAdd(data.ProductId, data);
    }

    public bool PostProcess(RShopProduct data)
    {
        return data.Currency != CurrencyType.None && data.Price > 0;
    }

    public RShopProduct? Find(int productId)
    {
        return _products.GetValueOrDefault(productId);
    }
}
