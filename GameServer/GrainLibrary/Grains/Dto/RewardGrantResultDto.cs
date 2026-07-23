using SharedLibrary;

namespace GrainLibrary.Grains.Dto;

public class CurrencyGrantResult
{
    public CurrencyType CurrencyType;
    public long Requested;
    public long Granted;
    public long Discarded => Requested - Granted;
    public ResultCode ResultCode;
}

public class ItemGrantResult
{
    public int ItemId;
    public int Requested;
    public int Granted;
    public int Discarded => Requested - Granted;
    public ResultCode ResultCode;
}

public class RewardGrantResult
{
    public List<CurrencyGrantResult> CurrencyGrants = [];
    public List<ItemGrantResult> ItemGrants = [];
    public bool HasAnyDiscarded => CurrencyGrants.Any(c => c.Discarded > 0) || ItemGrants.Any(i => i.Discarded > 0);
}