using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_levels")]
public record PlayerLevelRow
{
    public long player_id { get; set; }

    public int level { get; set; }

    public long exp { get; set; }

    public DateTime updated_at { get; set; }

    public DateTime created_at { get; set; }
}
