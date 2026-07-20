using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains.Dto;

public class MailClaimResultDto
{
    public ResultCode ResultCode { get; set; }

    public MailInfo? MailInfo { get; set; }

    public List<WalletInfo> WalletInfo { get; set; } = new();
}
