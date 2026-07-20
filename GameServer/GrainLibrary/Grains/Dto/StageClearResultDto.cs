using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains.Dto;

public class StageClearResultDto
{
    public ResultCode ResultCode { get; set; }

    public StageInfo? StageInfo { get; set; }

    public List<WalletInfo> WalletInfo { get; set; } = new();

    public int Level { get; set; }

    public long Exp { get; set; }
}
