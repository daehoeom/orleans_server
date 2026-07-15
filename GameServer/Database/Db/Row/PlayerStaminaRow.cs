using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_stamina")]
public record PlayerStaminaRow
{
    public long player_id { get; set; }

    public int amount { get; set; }

    public int max_amount { get; set; }

    public DateTime last_updated_at { get; set; }

    public DateTime created_at { get; set; }
}
