using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains.Dto;

public class StageClearResultDto
{
    public ResultCode ResultCode { get; set; }

    public StageInfoModel? StageInfo { get; set; }

    public List<WalletModel> WalletInfo { get; set; } = new();

    public int Level { get; set; }

    public long Exp { get; set; }

    public RewardGrantResult RewardGrant { get; set; } = new();
}
