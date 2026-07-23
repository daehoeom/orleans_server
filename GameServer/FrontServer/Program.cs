using Database.Db;
using Database.Redis;
using GameServer.Controllers;
using GrainLibrary.Logging;
using GrainLibrary.Resource;
using GrainLibrary.Server;
using GrainLibrary.Services;
using Serilog;

namespace GameServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = LoggerBootstrap.CreateBootstrapLogger("FrontServer");

        try
        {
            await Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseOrleans(siloBuilder => { siloBuilder.UseLocalhostClustering(); })
                .ConfigureServices(services =>
                {
                    // 세션
                    services.AddSingleton<DatabaseService>();
                    services.AddSingleton<RedisService>();
                    services.AddSingleton<SessionService>();

                    // 테이블 데이터
                    services.AddSingleton<ResourceLoader>();

                    // 패킷 컨트롤러 (역할별로 추가)
                    services.AddSingleton<PlayerBaseController, AuthController>();
                    services.AddSingleton<PlayerBaseController, PlayerController>();
                    services.AddSingleton<PlayerBaseController, ShopController>();
                    services.AddSingleton<PlayerBaseController, CommunityController>();
                    services.AddSingleton<PlayerBaseController, GachaController>();
                    services.AddSingleton<PlayerBaseController, StageController>();
                    services.AddSingleton<PlayerBaseController, AttendanceController>();
                    services.AddSingleton<PlayerBaseController, MailController>();

                    // 패킷 디스패처 & DotNetty 핸들러
                    services.AddSingleton<PacketHandler>();
                    services.AddSingleton<GameServerHandler>();

                    // DotNetty 서버
                    services.AddHostedService<ServerRunner>();
                })
                .RunConsoleAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "FrontServer terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}