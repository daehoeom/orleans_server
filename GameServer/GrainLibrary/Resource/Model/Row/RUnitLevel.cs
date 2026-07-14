using GrainLibrary.Resource.Attribute;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("UnitLevel")]
public class RUnitLevel
{
    public int Id { get; set; }

    public int UnitId { get; set; }

    public int Level { get; set; }

    public int RequireStack { get; set; }

    public CurrencyType RequireCurrencyType { get; set; }

    public int RequireCurrencyAmount { get; set; }
}
