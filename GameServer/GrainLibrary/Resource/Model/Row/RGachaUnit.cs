using GrainLibrary.Resource.Attribute;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("GachaUnit")]
public class RGachaUnit
{
    public int Id { get; set; }

    public int GachaId { get; set; }

    public int UnitId { get; set; }

    public int Weight { get; set; }
}
