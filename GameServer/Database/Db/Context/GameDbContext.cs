using Database.Db.DbSet;

namespace Database.Db.Context;

public class GameDbContext(DbConnector conn)
    : DbContext(conn)
{
    public PlayerDbSet Player { get; private set; } = new(conn);
    public PlayerWalletDbSet Wallets { get; private set; } = new(conn);
    public PlayerPurchaseLimitDbSet PurchaseLimit { get; private set; } = new(conn);
}