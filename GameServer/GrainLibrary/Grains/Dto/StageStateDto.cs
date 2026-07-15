namespace GrainLibrary.Grains.Dto;

public class StageStateDto
{
    public int StageIndex { get; set; }

    public bool MissionStep1 { get; set; }

    public bool MissionStep2 { get; set; }

    public bool MissionStep3 { get; set; }

    public short ClearScore { get; set; }
}
