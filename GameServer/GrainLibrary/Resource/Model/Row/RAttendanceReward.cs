using GrainLibrary.Resource.Attribute;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("AttendanceReward")]
public class RAttendanceReward
{
    public int RewardId { get; set; }

    public int EventId { get; set; }

    public int Day { get; set; }

    public CurrencyType RewardCurrencyType { get; set; }

    public long RewardCurrencyAmount { get; set; }

    public int RewardItemId { get; set; }

    public int RewardItemCount { get; set; }
}
