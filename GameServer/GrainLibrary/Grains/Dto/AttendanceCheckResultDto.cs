using SharedLibrary;

namespace GrainLibrary.Grains.Dto;

public class AttendanceCheckResultDto
{
    public ResultCode ResultCode { get; set; }

    public int EventId { get; set; }

    public int Day { get; set; }

    public bool Claimed { get; set; }
}
