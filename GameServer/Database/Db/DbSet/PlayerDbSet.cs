using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerDbSet(DbConnector conn)
{
    public Task<PlayerRow?> GetAsync(long player_id)
    {
        var sql = @"SELECT * FROM `players` 
                    WHERE `player_id` = @player_id";

        var param = new
        {
            player_id
        };
        return conn.QueryFirstOrDefaultAsync<PlayerRow>(sql, param);
    }

    public Task<int> InsertAsync(PlayerRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return conn.InsertAsync(row);
    }

    public async Task<int> UpdateNickNameAsync(long player_id, string changeName)
    {
        var now = DateTime.UtcNow;
        var sql = $@"UPDATE `players` SET `player_name` = @changeName, `updated_at` = @now
                    WHERE id = @player_id";

        var param = new
        {
            changeName,
            now,
            player_id,
        };

        return await conn.ExecuteAsync(sql, param);
    }

    public async Task<int> DeleteAsync(long player_id)
    {
        var sql = @"DELETE FROM `players` WHERE id = @player_id";

        var param = new
        {
            player_id,
        };

        return await conn.ExecuteAsync(sql, param);
    }
}