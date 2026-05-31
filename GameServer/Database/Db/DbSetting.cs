namespace Database.Db;

public record DbSetting
{
    public string AccountDbHost { get; init; } = string.Empty;
    public int AccountDbPort { get; init; }
    public string AccountDbDatabase { get; init; } = string.Empty;
    public string AccountDbUser { get; init; } = string.Empty;
    public string AccountDbPassWord { get; init; } = string.Empty;
    
    public string GameDbHost { get; init; } = string.Empty;
    public int GameDbPort { get; init; }
    public string GameDbDatabase { get; init; } = string.Empty;
    public string GameDbUser { get; init; } = string.Empty;
    public string GameDbPassWord { get; init; } = string.Empty;
}