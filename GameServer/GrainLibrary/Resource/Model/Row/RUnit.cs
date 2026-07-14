using GrainLibrary.Resource.Attribute;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("Unit")]
public class RUnit
{
    public int Id { get; set; }

    public UnitGradeType GradeType { get; set; }
}
