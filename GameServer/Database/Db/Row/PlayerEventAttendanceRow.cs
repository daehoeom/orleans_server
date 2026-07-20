using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_event_attendance")]
public record PlayerEventAttendanceRow
{
    public long player_id { get; set; }

    public int event_id { get; set; }

    public int day { get; set; }

    public DateTime last_updated_at { get; set; }

    public DateTime created_at { get; set; }
}
