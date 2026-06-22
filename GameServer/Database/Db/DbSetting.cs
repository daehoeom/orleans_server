namespace Database.Db;

public record DbSetting
{
    public string AccountDbHost { get; init; } = string.Empty;
    public int AccountDbPort { get; init; } = 3306;
    public string AccountDbDatabase { get; init; } = string.Empty;
    public string AccountDbUser { get; init; } = string.Empty;
    public string AccountDbPassWord { get; init; } = string.Empty;
    public int AccountDbMinPool { get; init; } = 30;
    public int AccountDbMaxPool { get; init; } = 60;
    
    public string GameDbHost { get; init; } = string.Empty;
    public int GameDbPort { get; init; } = 3306;
    public string GameDbDatabase { get; init; } = string.Empty;
    public string GameDbUser { get; init; } = string.Empty;
    public string GameDbPassWord { get; init; } = string.Empty;
    public int GameDbMinPool { get; init; } = 30;
    public int GameDbMaxPool { get; init; } = 60;
}