namespace GrainLibrary.Grains.Dto;

public class AttendanceStateDto
{
    public int EventId { get; set; }

    public int Day { get; set; }

    public bool Claimed { get; set; }

    public int MaxDay { get; set; }
}
