using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains.Dto;

public class MailReadResultDto
{
    public ResultCode ResultCode { get; set; }

    public MailInfo? MailInfo { get; set; }

    public List<WalletInfo> WalletInfo { get; set; } = new();

    public RewardGrantResult RewardGrant { get; set; } = new();
}
