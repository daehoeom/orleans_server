using Database.Db.Row;
using SharedLibrary;

namespace Database.Db.DbSet;

public class PlayerWalletDbSet(DbConnector conn)
{
    public async Task<PlayerWalletRow?> GetAsync(long player_id, int monetary_type)
    {
        var query = @"SELECT * FROM `player_wallets`
                    WHERE `player_id` = @player_id AND `monetary_type` = @monetary_type";
        var param = new
        {
            player_id,
            monetary_type,
        };

        return await conn.QueryFirstOrDefaultAsync<PlayerWalletRow>(query, param);
    }

    public async Task<PlayerWalletRow?> GetAsync(long player_id, CurrencyType monetary_type)
    {
        return await GetAsync(player_id, (int)monetary_type);
    }

    public async Task<IEnumerable<PlayerWalletRow>> GetsAsync(long player_id)
    {
        var query = @"SELECT * FROM `player_wallets`
                    WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
        };

        return await conn.QueryAsync<PlayerWalletRow>(query, param);
    }

    public async Task<int> InsertAsync(PlayerWalletRow walletRow)
    {
        walletRow.updated_at = DateTime.UtcNow;
        walletRow.created_at = DateTime.UtcNow;

        return await conn.InsertAsync(walletRow);
    }

    public async Task<int> AddAsync(long player_id, int monetary_type, long addValue)
    {
        var query = @"UPDATE `player_wallets` SET `amount` = `amount` +  @addValue
                WHERE `player_id` = @player_id AND `monetary_type` = @monetary_type";

        var param = new
        {
            player_id,
            monetary_type,
            addValue
        };

        return await conn.ExecuteAsync(query, param);
    }

    public async Task<int> SpendAsync(long player_id, int monetary_type, long spendValue)
    {
        var query = @"UPDATE `player_wallets` SET `amount` = `amount` - @spendValue
                WHERE `player_id` = @player_id AND `monetary_type` = @monetary_type";

        var param = new
        {
            player_id,
            monetary_type,
            spendValue,
        };

        return await conn.ExecuteAsync(query, param);
    }

    public async Task<int> UpdateAsync(long player_id, int monetary_type, long updateValue)
    {
        var query = @"UPDATE `player_wallets` SET `amount` = @updateValue
                WHERE `player_id` = @player_id AND `monetary_type` = @monetary_type";

        var param = new
        {
            player_id,
            monetary_type,
            updateValue,
        };

        return await conn.ExecuteAsync(query, param);
    }
}