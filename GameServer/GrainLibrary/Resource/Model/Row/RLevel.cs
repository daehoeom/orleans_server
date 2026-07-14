using GrainLibrary.Resource.Attribute;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("Level")]
public class RLevel
{
    public int Level { get; set; }

    public long RequiredExp { get; set; }
    
    public int NextLevel { get; set; }
}
