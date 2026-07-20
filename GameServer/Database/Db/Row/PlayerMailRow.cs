using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_mails")]
public record PlayerMailRow
{
    public long id { get; set; }

    public long mail_id { get; set; }

    public long player_id { get; set; }

    public int type { get; set; }

    public string title { get; set; } = string.Empty;

    public string body { get; set; } = string.Empty;

    public string rewards { get; set; } = "[]";

    public bool is_read { get; set; }

    public DateTime expired_at { get; set; }

    public DateTime updated_at { get; set; }

    public DateTime created_at { get; set; }
}
