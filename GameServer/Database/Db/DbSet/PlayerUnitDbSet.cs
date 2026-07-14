using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerUnitDbSet(DbConnector conn)
{
    public async Task<PlayerUnitRow?> GetAsync(long player_id, int unit_id)
    {
        var sql = @"SELECT * FROM `player_units`
                WHERE `player_id` = @player_id AND `unit_id` = @unit_id";

        var param = new
        {
            player_id,
            unit_id,
        };

        return await conn.QueryFirstOrDefaultAsync<PlayerUnitRow>(sql, param);
    }

    public async Task<IEnumerable<PlayerUnitRow>> GetsAsync(long player_id)
    {
        var sql = @"SELECT * FROM `player_units`
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
        };

        return await conn.QueryAsync<PlayerUnitRow>(sql, param);
    }

    public async Task<int> InsertAsync(PlayerUnitRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return await conn.InsertAsync(row);
    }

    public async Task<int> UpdateLevelAsync(long player_id, int unit_id, int level)
    {
        var sql = @"UPDATE `player_units` SET `level` = @level, `updated_at` = @updated_at
                WHERE `player_id` = @player_id AND `unit_id` = @unit_id";

        var param = new
        {
            player_id,
            unit_id,
            level,
            updated_at = DateTime.UtcNow,
        };

        return await conn.ExecuteAsync(sql, param);
    }

    public async Task<int> AddStackAsync(long player_id, int unit_id)
    {
        var sql = @"UPDATE `player_units` SET `stack` = `stack` + 1, `updated_at` = @updated_at
                WHERE `player_id` = @player_id AND `unit_id` = @unit_id";

        var param = new
        {
            player_id,
            unit_id,
            updated_at = DateTime.UtcNow,
        };

        return await conn.ExecuteAsync(sql, param);
    }
    
    public async Task<int> DeleteAsync(long player_id, int unit_id)
    {
        var sql = @"DELETE FROM `player_units`
                WHERE `player_id` = @player_id AND `unit_id` = @unit_id";

        var param = new
        {
            player_id,
            unit_id,
        };

        return await conn.ExecuteAsync(sql, param);
    }
}
