using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains.Dto;

public class AttendanceClaimResultDto
{
    public ResultCode ResultCode { get; set; }

    public int EventId { get; set; }

    public int Day { get; set; }

    public CurrencyType RewardCurrencyType { get; set; }

    public long RewardCurrencyAmount { get; set; }

    public int RewardItemId { get; set; }

    public int RewardItemCount { get; set; }

    public List<WalletInfo> WalletInfo { get; set; } = new();

    public RewardGrantResult RewardGrant { get; set; } = new();
}
