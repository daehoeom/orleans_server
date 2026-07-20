using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_event_attendance_rewards")]
public record PlayerEventAttendanceRewardRow
{
    public long id { get; set; }

    public long player_id { get; set; }

    public int event_id { get; set; }

    public int reward_id { get; set; }

    public bool received_flag { get; set; }

    public DateTime updated_at { get; set; }

    public DateTime created_at { get; set; }
}
