using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace ServerLibrary.Server;

public class TcpServerRunner
{
    public static async Task RunServer(string host, int port)
    {
        var cts = new CancellationTokenSource();
        
        // 1. 스레드 그룹 생성 (Boss: 연결 수락, Worker: 데이터 송수신)
        IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
        IEventLoopGroup workerGroup = new MultithreadEventLoopGroup();

        try
        {
            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup, workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast("echoHandler", new TcpServerHandler());
                }));

            var boundChannel = await bootstrap.BindAsync(port);
            Console.WriteLine($"[Server] Server running on port {host}:{port}");

            while (true)
            {
                
            }

            await boundChannel.CloseAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Server] Exception: {e.Message}");
        }
        finally
        {
            await Task.WhenAll(
                bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
            );
            Console.WriteLine("[Server] Shutdown server process gracefully.");
        }
    }
}