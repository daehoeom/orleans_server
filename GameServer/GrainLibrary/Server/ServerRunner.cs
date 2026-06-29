using Database.Db;
using Database.Redis;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GrainLibrary.Services;

namespace GrainLibrary.Server;

public class ServerRunner(
    ILogger<ServerRunner> logger,
    DatabaseService databaseService,
    RedisService redisService,
    GameServerHandler handler)
    : BackgroundService
{
    private IEventLoopGroup _bossGroup = null!;
    private IEventLoopGroup _workerGroup = null!;
    private IChannel _boundChannel = null!;

    private async Task<bool> BindServerAsync(string host, int port)
    {
        _bossGroup = new MultithreadEventLoopGroup(1);
        _workerGroup = new MultithreadEventLoopGroup();

        try
        {
            var bootStrap = new ServerBootstrap();
            bootStrap
                .Group(_bossGroup, _workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;

                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(
                        maxFrameLength: 1 << 20,
                        lengthFieldOffset: 0,
                        lengthFieldLength: 4,
                        lengthAdjustment: 0,
                        initialBytesToStrip: 4));

                    pipeline.AddLast(new PacketDecoder());

                    pipeline.AddLast(new PacketEncoder());

                    pipeline.AddLast(handler);
                }));

            _boundChannel = await bootStrap.BindAsync(port);
            logger.LogInformation($"[Server] Server running on port {host}:{port}");
        }
        catch (Exception e)
        {
            logger.LogError($"[Server] Exception: {e.Message}");
            throw;
        }

        return true;
    }

    private async Task CloseAsync()
    {
        logger.LogInformation("[Server] Stopping Netty Server...");

        await _boundChannel.CloseAsync();
        
        await Task.WhenAll(
            _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
            _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
        );

        logger.LogInformation("[Server] Shutdown server process gracefully.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _boundChannel.CloseCompletion;
        }
        catch (Exception ex)
        {
            logger.LogError($"[Server] Error during waiting for channel completion: {ex.Message}");
        }
        
        logger.LogInformation("[Server] Server channel closed. Stepping out from ExecuteAsync.");
    }

    public override async Task StartAsync(CancellationToken _)
    {
        await databaseService.CheckConnectionAsync();
        await redisService.ConnectAsync();

        var isBindSuccess = await BindServerAsync("127.0.0.1", 35000);
        if (!isBindSuccess)
        {
            throw new Exception("Bind server failed");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await CloseAsync();
    }
}