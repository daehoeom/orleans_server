namespace Database.Db.Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute(string name) : System.Attribute
{
    public string TableName { get; } = name;
}