using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerPurchaseLimitDbSet(DbConnector conn)
{
    public async Task<PlayerPurchaseLimitRow?> GetAsync(long player_id, int product_id)
    {
        var sql = @"SELECT * FROM `player_purchase_limit` 
                WHERE `player_id` = @player_id AND `product_id` = @product_id";

        var param = new
        {
            player_id,
            product_id,
        };

        return await conn.QueryFirstOrDefaultAsync<PlayerPurchaseLimitRow>(sql, param);
    }

    public async Task<IEnumerable<PlayerPurchaseLimitRow>> GetsAsync(long player_id)
    {
        var sql = @"SELECT * FROM `player_purchase_limit`
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
        };

        return await conn.QueryAsync<PlayerPurchaseLimitRow>(sql, param);
    }

    public async Task<int> InsertAsync(PlayerPurchaseLimitRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return await conn.InsertAsync(row);
    }

    public async Task<int> AddAsync(long player_id, int product_id, int addValue)
    {
        var sql = @"UPDATE `player_purchase_limit` SET `purchase_count` = `purchase_count` + @addValue
                WHERE `player_id` = @player_id AND `product_id` = @product_id";

        var param = new
        {
            player_id,
            product_id,
            addValue,
        };

        return await conn.ExecuteAsync(sql, param);
    }
}