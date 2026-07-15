using Database.Db.DbSet;

namespace Database.Db.Context;

public class GameDbContext(DbConnector conn)
    : DbContext(conn)
{
    public PlayerDbSet Player { get; private set; } = new(conn);
    public PlayerWalletDbSet Wallets { get; private set; } = new(conn);
    public PlayerPurchaseLimitDbSet PurchaseLimit { get; private set; } = new(conn);
    public PlayerUnitDbSet Units { get; private set; } = new(conn);
    public PlayerInventoryDbSet Inventory { get; private set; } = new(conn);
    public PlayerLevelDbSet Level { get; private set; } = new(conn);
    public PlayerStageStateDbSet StageStates { get; private set; } = new(conn);
}