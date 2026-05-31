using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_wallets")]
public class PlayerWalletRow
{
    public long id { get; set; }

    public long player_id { get; set; }

    public int monetary_type { get; set; }

    public long amount { get; set; }

    public DateTime updated_at { get; set; }

    public DateTime created_at { get; set; }
}