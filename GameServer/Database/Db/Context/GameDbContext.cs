using Database.Db.DbSet;

namespace Database.Db.Context;

public class GameDbContext(DbConnector conn)
    : DbContext(conn)
{
    public readonly PlayerDbSet Player = new(conn);
    
    
}