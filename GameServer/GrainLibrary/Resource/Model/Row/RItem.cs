using GrainLibrary.Resource.Attribute;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.Row;

[ResourceTable("Item")]
public class RItem
{
    public int Id { get; set; }
    
    public ItemType ItemType { get; set; }
    
    public int MaxStack { get; set; }
}