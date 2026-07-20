using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerDbSet(DbConnector conn)
{
    public Task<PlayerRow?> GetAsync(long player_id)
    {
        var query = @"SELECT * FROM `players` 
                    WHERE `player_id` = @player_id";

        var param = new
        {
            player_id
        };
        return conn.QueryFirstOrDefaultAsync<PlayerRow>(query, param);
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
        var query = $@"UPDATE `players` SET `player_name` = @changeName, `updated_at` = @now
                    WHERE id = @player_id";

        var param = new
        {
            changeName,
            now,
            player_id,
        };

        return await conn.ExecuteAsync(query, param);
    }

    public async Task<int> DeleteAsync(long player_id)
    {
        var query = @"DELETE FROM `players` WHERE id = @player_id";

        var param = new
        {
            player_id,
        };

        return await conn.ExecuteAsync(query, param);
    }
}