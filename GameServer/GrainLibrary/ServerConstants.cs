namespace GrainLibrary;

public enum DbType
{
    Account,
    Game,
}

public enum RedisDbType
{
    Account,
}

public static class ServerConstants
{
    public static readonly DateTime InfinityTime = new DateTime(2099, 12, 31);
}