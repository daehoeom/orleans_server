using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerStageStateDbSet(DbConnector conn)
{
    public async Task<PlayerStageStateRow?> GetAsync(long player_id, int stage_index)
    {
        var query = @"SELECT * FROM `player_stage_states`
                WHERE `player_id` = @player_id AND `stage_index` = @stage_index";

        var param = new
        {
            player_id,
            stage_index,
        };

        return await conn.QueryFirstOrDefaultAsync<PlayerStageStateRow>(query, param);
    }

    public async Task<IEnumerable<PlayerStageStateRow>> GetsAsync(long player_id)
    {
        var query = @"SELECT * FROM `player_stage_states`
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
        };

        return await conn.QueryAsync<PlayerStageStateRow>(query, param);
    }

    public async Task<int> InsertAsync(PlayerStageStateRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return await conn.InsertAsync(row);
    }

    public async Task<int> UpdateAsync(
        long player_id,
        int stage_index,
        bool mission_step_1,
        bool mission_step_2,
        bool mission_step_3,
        short clear_score)
    {
        var query = @"UPDATE `player_stage_states` SET
                `mission_step_1` = @mission_step_1,
                `mission_step_2` = @mission_step_2,
                `mission_step_3` = @mission_step_3,
                `clear_score` = @clear_score,
                `updated_at` = @updated_at
                WHERE `player_id` = @player_id AND `stage_index` = @stage_index";

        var param = new
        {
            player_id,
            stage_index,
            mission_step_1,
            mission_step_2,
            mission_step_3,
            clear_score,
            updated_at = DateTime.UtcNow,
        };

        return await conn.ExecuteAsync(query, param);
    }
}
