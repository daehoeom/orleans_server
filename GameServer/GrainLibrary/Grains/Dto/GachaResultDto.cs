using SharedLibrary;

namespace GrainLibrary.Grains.Dto;

public class GachaResultDto
{
    public ResultCode ResultCode { get; set; }

    public List<GachaUnitResultDto> Units { get; set; } = new();
}
