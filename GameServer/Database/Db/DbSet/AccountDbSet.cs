using Database.Db.Row;

namespace Database.Db.DbSet;

public class AccountDbSet(DbConnector conn)
{
    public Task<AccountRow?> GetAsync(string guid)
    {
        var query = @"SELECT * FROM `accounts` WHERE `guid` = @guid";

        var param = new
        {
            guid,
        };

        return conn.QueryFirstOrDefaultAsync<AccountRow>(query, param);
    }

    public Task<int> InsertAsync(AccountRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return conn.InsertAsync(row);
    }
}