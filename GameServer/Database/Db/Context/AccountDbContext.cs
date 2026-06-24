using Database.Db.DbSet;

namespace Database.Db.Context;

public class AccountDbContext(DbConnector conn)
    : DbContext(conn)
{
    public AccountDbSet Account { get; private set; } = new(conn);
}