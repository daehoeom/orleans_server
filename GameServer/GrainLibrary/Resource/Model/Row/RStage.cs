using GrainLibrary.Resource.Attribute;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("Stage")]
public class RStage
{
    public int StageId { get; set; }

    public int StaminaCost { get; set; }

    public int RewardExp { get; set; }

    public CurrencyType RewardCurrencyType { get; set; }

    public int RewardCurrencyAmount { get; set; }
}
