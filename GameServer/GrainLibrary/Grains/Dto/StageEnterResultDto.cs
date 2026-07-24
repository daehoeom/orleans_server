using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains.Dto;

public class StageEnterResultDto
{
    public ResultCode ResultCode { get; set; }

    public StaminaModel? StaminaInfo { get; set; }
}
