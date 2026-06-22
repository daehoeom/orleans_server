using System.Diagnostics;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using ScenarioBot.Scenario;

namespace ScenarioBot.Session;

public record BotResult(long BotId, bool Success, TimeSpan ElapsedTime, string? Error = null);

public class BotManager(string host, int port, IScenario scenario, int botCount)
{
    private IEventLoopGroup _eventLoopGroup = null!;

    public async Task<List<BotResult>> RunAsync()
    {
        _eventLoopGroup = new MultithreadEventLoopGroup();

        try
        {
            var tasks = Enumerable.Range(1, botCount)
                .Select(id => RunBotAsync(id))
                .ToList();

            var results = await Task.WhenAll(tasks);
            PrintSummary(results);
            return results.ToList();
        }
        finally
        {
            await _eventLoopGroup.ShutdownGracefullyAsync(
                TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }
    }

    private async Task<BotResult> RunBotAsync(long botId)
    {
        var session = new BotClientSession(botId);
        var stopWatch = Stopwatch.StartNew();

        try
        {
            var channel = await ConnectAsync(session);
            await scenario.RunAsync(session);
            await channel.CloseAsync();

            stopWatch.Stop();

            return new BotResult(botId, true, stopWatch.Elapsed);
        }
        catch (Exception e)
        {
            stopWatch.Stop();
            return new BotResult(botId, false, stopWatch.Elapsed, e.Message);
        }
    }

    private Task<IChannel> ConnectAsync(BotClientSession session)
    {
        var bootstrap = new Bootstrap()
            .Group(_eventLoopGroup)
            .Channel<TcpSocketChannel>()
            .Handler(new ActionChannelInitializer<IChannel>(channel =>
            {
                var pipeline = channel.Pipeline;
 
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(
                    maxFrameLength:      1 << 20,
                    lengthFieldOffset:   0,
                    lengthFieldLength:   4,
                    lengthAdjustment:    0,
                    initialBytesToStrip: 4));
 
                pipeline.AddLast(new BotPacketDecoder());
                pipeline.AddLast(new BotClientHandler(session));
            }));
 
        return bootstrap.ConnectAsync(host, port);
    }

    
    private static void PrintSummary(BotResult[] results)
    {
        var success = results.Count(r => r.Success);
        var failed = results.Length - success;
        var avgMs = results.Average(r => r.ElapsedTime.TotalMilliseconds);
        var maxMs = results.Max(r => r.ElapsedTime.TotalMilliseconds);
        var minMs = results.Min(r => r.ElapsedTime.TotalMilliseconds);

        Console.WriteLine("──────────────────────────────");
        Console.WriteLine($"총 봇:   {results.Length}");
        Console.WriteLine($"성공:    {success}");
        Console.WriteLine($"실패:    {failed}");
        Console.WriteLine($"평균 응답: {avgMs:F1} ms");
        Console.WriteLine($"최소 응답: {minMs:F1} ms");
        Console.WriteLine($"최대 응답: {maxMs:F1} ms");
        Console.WriteLine("──────────────────────────────");

        foreach (var r in results.Where(r => !r.Success))
        {
            Console.WriteLine($"  Bot#{r.BotId} 실패: {r.Error}");
        }
    }
}