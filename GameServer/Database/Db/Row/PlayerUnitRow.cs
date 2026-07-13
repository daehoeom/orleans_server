using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_units")]
public class PlayerUnitRow
{
    public long id { get; set; }

    public long player_id { get; set; }

    public int unit_id { get; set; }

    public int level { get; set; }

    public int grade { get; set; }

    public DateTime updated_at { get; set; }

    public DateTime created_at { get; set; }
}
