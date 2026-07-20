using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerMailDbSet(DbConnector conn)
{
    public Task<IEnumerable<PlayerMailRow>> GetsAsync(long player_id)
    {
        var query = @"SELECT * FROM `player_mails`
                WHERE `player_id` = @player_id AND `expired_at` > @now";

        var param = new
        {
            player_id,
            now = DateTime.UtcNow,
        };

        return conn.QueryAsync<PlayerMailRow>(query, param);
    }

    public Task<int> UpdateReadAsync(long player_id, long id)
    {
        var query = @"UPDATE `player_mails` SET
                `is_read` = 1,
                `updated_at` = @updated_at
                WHERE `player_id` = @player_id AND `id` = @id";

        var param = new
        {
            player_id,
            id,
            updated_at = DateTime.UtcNow,
        };

        return conn.ExecuteAsync(query, param);
    }

    public Task<int> UpdateRewardsAsync(long player_id, long id, string rewards)
    {
        var query = @"UPDATE `player_mails` SET
                `rewards` = @rewards,
                `is_read` = 1,
                `updated_at` = @updated_at
                WHERE `player_id` = @player_id AND `id` = @id";

        var param = new
        {
            player_id,
            id,
            rewards,
            updated_at = DateTime.UtcNow,
        };

        return conn.ExecuteAsync(query, param);
    }

    public Task<int> DeleteAsync(long player_id, long id)
    {
        var query = @"DELETE FROM `player_mails`
                WHERE `player_id` = @player_id AND `id` = @id";

        var param = new
        {
            player_id,
            id,
        };

        return conn.ExecuteAsync(query, param);
    }

    public Task<int> DeleteAllAsync(long player_id)
    {
        var query = @"DELETE FROM `player_mails` 
                WHERE `player_id` = @player_id AND `is_read` = 1";

        var param = new
        {
            player_id,
        };
        
        return conn.ExecuteAsync(query, param);
    }
}
