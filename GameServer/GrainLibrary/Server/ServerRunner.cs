using Database.Db;
using Database.Redis;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerLibrary.Services;

namespace ServerLibrary.Server;

public class ServerRunner : BackgroundService
{
    private readonly ILogger<ServerRunner> _logger;
    private readonly DatabaseService _databaseService;
    private readonly RedisService _redisService;
    private readonly GameServerHandler _gameServerHandler;

    private IEventLoopGroup _bossGroup;
    private IEventLoopGroup _workerGroup;
    private IChannel _boundChannel;
    
    public ServerRunner(ILogger<ServerRunner> logger, DatabaseService databaseService, 
        RedisService redisService, GameServerHandler handler)
    {
        _logger = logger;
        _databaseService = databaseService;
        _redisService = redisService;
        _gameServerHandler = handler;
    }

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

                    pipeline.AddLast(_gameServerHandler);
                }));

            _boundChannel = await bootStrap.BindAsync(port);
            _logger.LogInformation($"[Server] Server running on port {host}:{port}");
        }
        catch (Exception e)
        {
            _logger.LogError($"[Server] Exception: {e.Message}");
            throw;
        }

        return true;
    }

    private async Task CloseAsync()
    {
        _logger.LogInformation("[Server] Stopping Netty Server...");

        await _boundChannel.CloseAsync();
        
        await Task.WhenAll(
            _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
            _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
        );

        _logger.LogInformation("[Server] Shutdown server process gracefully.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _boundChannel.CloseCompletion;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Server] Error during waiting for channel completion: {ex.Message}");
        }
        
        _logger.LogInformation("[Server] Server channel closed. Stepping out from ExecuteAsync.");
    }

    public override async Task StartAsync(CancellationToken _)
    {
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