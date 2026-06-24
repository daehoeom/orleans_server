using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("accounts")]
public record AccountRow
{
    public long account_id { get; set; }
    
    public string guid { get; set; }
    
    public DateTime updated_at { get; set; }
    
    public DateTime created_at { get; set; }
}