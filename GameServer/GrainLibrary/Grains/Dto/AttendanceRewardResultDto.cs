using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains.Dto;

public class AttendanceRewardResultDto
{
    public ResultCode ResultCode { get; set; }

    public int EventId { get; set; }

    public int Day { get; set; }

    public RewardResultModel RewardResult { get; set; } = new();

    public RewardGrantResult RewardGrant { get; set; } = new();
}
