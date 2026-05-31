namespace Database.Redis;

public record RedisSetting
{
    public string Host { get; set; } = "127.0.0.1";
    
    public int Port { get; set; } = 6379;

    public string Password { get; set; } = "";
    
    public int ConnectTimeout { get; set; } = 60;
    
    public int SyncTimeout { get; set; } = 60;
    
    public int DefaultDatabase { get; set; } = 0;
}