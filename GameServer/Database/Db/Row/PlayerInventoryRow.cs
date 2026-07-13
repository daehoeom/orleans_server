using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_inventory")]
public class PlayerInventoryRow
{
    public long id { get; set; }

    public long player_id { get; set; }

    public int item_id { get; set; }

    public int count { get; set; }

    public DateTime updated_at { get; set; }

    public DateTime created_at { get; set; }
}
