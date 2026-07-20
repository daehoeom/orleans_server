using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerLevelDbSet(DbConnector conn)
{
    public async Task<PlayerLevelRow?> GetAsync(long player_id)
    {
        var query = @"SELECT * FROM `player_levels`
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
        };

        return await conn.QueryFirstOrDefaultAsync<PlayerLevelRow>(query, param);
    }

    public async Task<int> InsertAsync(PlayerLevelRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return await conn.InsertAsync(row);
    }

    public async Task<int> UpdateLevelAsync(long player_id, int level, long exp)
    {
        var query = @"UPDATE `player_levels` SET `level` = @level, `exp` = @exp, `updated_at` = @updated_at
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
            level,
            exp,
            updated_at = DateTime.UtcNow,
        };

        return await conn.ExecuteAsync(query, param);
    }
}
