using GrainLibrary.Resource.Attribute;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("UnitGrade")]
public class RUnitGrade
{
    public int Grade { get; set; }

    public int RequireStack { get; set; }
    
    public CurrencyType RequireCurrencyType { get; set; }
    
    public int RequireCurrencyAmount { get; set; }

    public int NextGrade { get; set; }
}
