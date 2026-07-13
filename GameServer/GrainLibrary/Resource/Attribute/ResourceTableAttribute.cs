namespace GrainLibrary.Resource.Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class ResourceTableAttribute(string name) : System.Attribute
{
    public string TableName { get; } = name;
}
