using GrainLibrary.Resource.Attribute;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("ShopProduct")]
public class RShopProduct
{
    public int ProductId { get; set; }

    public CurrencyType Currency { get; set; }

    public int Price { get; set; }

    public int LimitCount { get; set; }

    public int MaxPurchaseCount { get; set; }
    
    public int RewardItemId { get; set; }
    
    public int RewardItemCount { get; set; }
}
