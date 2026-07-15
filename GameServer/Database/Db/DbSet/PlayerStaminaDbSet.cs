using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerStaminaDbSet(DbConnector conn)
{
    public async Task<PlayerStaminaRow?> GetAsync(long player_id)
    {
        var sql = @"SELECT * FROM `player_stamina`
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
        };

        return await conn.QueryFirstOrDefaultAsync<PlayerStaminaRow>(sql, param);
    }

    public async Task<int> InsertAsync(PlayerStaminaRow row)
    {
        row.last_updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return await conn.InsertAsync(row);
    }

    public async Task<int> UpdateAsync(long player_id, int amount, DateTime last_updated_at)
    {
        var sql = @"UPDATE `player_stamina` SET `amount` = @amount, `last_updated_at` = @last_updated_at
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
            amount,
            last_updated_at,
        };

        return await conn.ExecuteAsync(sql, param);
    }
}
