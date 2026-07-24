namespace GrainLibrary.Grains.Dto;

public class AttendanceStateDto
{
    public int EventId { get; set; }
    
    public int CurrentDay { get; set; }

    public Dictionary<int, bool> RewardReceivedFlag;
}
