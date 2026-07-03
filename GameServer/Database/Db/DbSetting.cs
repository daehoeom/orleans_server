namespace Database.Db;

public record DbSetting
{
    public string AccountDbHost { get; init; } = "127.0.0.1";
    public int AccountDbPort { get; init; } = 3306;
    public string AccountDbDatabase { get; init; } = "account";
    public string AccountDbUser { get; init; } = "clover";
    public string AccountDbPassWord { get; init; } = "2rjt2fkwnffl";
    public int AccountDbMinPool { get; init; } = 30;
    public int AccountDbMaxPool { get; init; } = 60;
    
    public string GameDbHost { get; init; } = "127.0.0.1";
    public int GameDbPort { get; init; } = 3306;
    public string GameDbDatabase { get; init; } = "game";
    public string GameDbUser { get; init; } = "clover";
    public string GameDbPassWord { get; init; } = "2rjt2fkwnffl";
    public int GameDbMinPool { get; init; } = 30;
    public int GameDbMaxPool { get; init; } = 60;
}