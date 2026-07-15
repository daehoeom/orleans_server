using Database.Db;
using Database.Redis;
using GameServer.Controllers;
using GrainLibrary.Resource;
using GrainLibrary.Server;
using GrainLibrary.Services;

namespace GameServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .UseOrleans(siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering();
            })
            .ConfigureServices(services =>
            {
                // 세션
                services.AddSingleton<DatabaseService>();
                services.AddSingleton<RedisService>();
                services.AddSingleton<SessionService>();

                // 테이블 데이터
                services.AddSingleton<ResourceLoader>();
 
                // 패킷 컨트롤러 (역할별로 추가)
                services.AddSingleton<PlayerBaseController, PlayerController>();
                services.AddSingleton<PlayerBaseController, ShopController>();
                services.AddSingleton<PlayerBaseController, CommunityController>();
                services.AddSingleton<PlayerBaseController, GachaController>();
 
                // 패킷 디스패처 & DotNetty 핸들러
                services.AddSingleton<PacketHandler>();
                services.AddSingleton<GameServerHandler>();
 
                // DotNetty 서버
                services.AddHostedService<ServerRunner>();
            })
            .RunConsoleAsync();
    }
}