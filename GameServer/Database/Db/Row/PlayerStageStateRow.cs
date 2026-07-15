using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_stage_states")]
public record PlayerStageStateRow
{
    public long id { get; set; }

    public long player_id { get; set; }

    public int stage_index { get; set; }

    public bool mission_step_1 { get; set; }

    public bool mission_step_2 { get; set; }

    public bool mission_step_3 { get; set; }

    public short clear_score { get; set; }

    public DateTime updated_at { get; set; }

    public DateTime created_at { get; set; }
}
