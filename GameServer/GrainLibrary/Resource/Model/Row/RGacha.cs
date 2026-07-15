using GrainLibrary.Resource.Attribute;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("Gacha")]
public class RGacha
{
    public int GachaId { get; set; }

    public CurrencyType CostCurrencyType { get; set; }

    public int CostAmount { get; set; }
}
