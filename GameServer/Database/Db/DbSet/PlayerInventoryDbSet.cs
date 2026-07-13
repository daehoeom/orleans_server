using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerInventoryDbSet(DbConnector conn)
{
    public async Task<PlayerInventoryRow?> GetAsync(long player_id, int item_id)
    {
        var sql = @"SELECT * FROM `player_inventory`
                WHERE `player_id` = @player_id AND `item_id` = @item_id";

        var param = new
        {
            player_id,
            item_id,
        };

        return await conn.QueryFirstOrDefaultAsync<PlayerInventoryRow>(sql, param);
    }

    public async Task<IEnumerable<PlayerInventoryRow>> GetsAsync(long player_id)
    {
        var sql = @"SELECT * FROM `player_inventory`
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
        };

        return await conn.QueryAsync<PlayerInventoryRow>(sql, param);
    }

    public async Task<int> InsertAsync(PlayerInventoryRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return await conn.InsertAsync(row);
    }

    public async Task<int> AddAsync(long player_id, int item_id, int addValue)
    {
        var sql = @"UPDATE `player_inventory` SET `count` = `count` + @addValue, `updated_at` = @updated_at
                WHERE `player_id` = @player_id AND `item_id` = @item_id";

        var param = new
        {
            player_id,
            item_id,
            addValue,
            updated_at = DateTime.UtcNow,
        };

        return await conn.ExecuteAsync(sql, param);
    }

    public async Task<int> SpendAsync(long player_id, int item_id, int spendValue)
    {
        var sql = @"UPDATE `player_inventory` SET `count` = `count` - @spendValue, `updated_at` = @updated_at
                WHERE `player_id` = @player_id AND `item_id` = @item_id";

        var param = new
        {
            player_id,
            item_id,
            spendValue,
            updated_at = DateTime.UtcNow,
        };

        return await conn.ExecuteAsync(sql, param);
    }
}
