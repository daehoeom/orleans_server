using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("players")]
public record PlayerRow
{
    public long player_id { get; set; }
    
    public string player_name { get; set; }
    
    public int player_thumbnail { get; set; }
    
    public DateTime updated_at { get; set; }
    
    public DateTime created_at { get; set; }
}