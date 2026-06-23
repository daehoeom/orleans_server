using System.Reflection;
using DotNetty.Transport.Channels;
using MessagePack;
using Microsoft.Extensions.Logging;
using SharedLibrary;
using SharedLibrary.Packet.Base;

namespace ServerLibrary.Services;

public class PacketHandler
{
    private readonly ILogger<PacketHandler> _logger;
    private readonly SessionService _sessionService;
    private readonly Dictionary<PacketHeaderType, Router> _handlers = new();

    public PacketHandler(ILogger<PacketHandler> logger, SessionService sessionService, 
        IEnumerable<PlayerBaseController> controllers)
    {
        _logger = logger;
        _sessionService = sessionService;
        foreach (var controller in controllers)
        {
            RegisterHandler(controller);
        }
    }

    public void RegisterHandler(object target)
    {
        foreach (var method in target.GetType()
                     .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var attr = method.GetCustomAttribute<PacketHandlerAttribute>();
            if (attr is null)
            {
                continue;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 2)
            {
                throw new InvalidOperationException(
                    $"{method.Name}: 시그니처는 (IChannelHandlerContext, TReq) 이어야 합니다.");
            }

            var requestType = parameters[1].ParameterType;
            if (_handlers.ContainsKey(attr.HeaderType))
            {
                throw new InvalidOperationException(
                    $"HeaderType {attr.HeaderType} 핸들러가 중복 등록되었습니다.");
            }

            _handlers[attr.HeaderType] = new Router
            {
                Type = requestType,
                Target = target,
                MethodInfo = method,
            };
            
            Console.WriteLine($"[PacketHandler] 등록: {attr.HeaderType} → {method.Name} ({requestType.Name})");
        }
    }

    public async Task DispatchAsync(IChannelHandlerContext context, StreamPacket stream)
    {
        var session = _sessionService.GetSession(context);
        if (session is null)
        {
            Console.WriteLine($"[PacketHandler] 세션 없음: {context.Channel.RemoteAddress}");
            await context.CloseAsync();
            return;
        }
        
        if (!_handlers.TryGetValue(stream.HeaderType, out var router))
        {
            Console.WriteLine($"[PacketHandler] 미등록 HeaderType: {stream.HeaderType}");
            return;
        }

        var request = MessagePackSerializer.Deserialize(router.Type, stream.Body);
        var result = router.MethodInfo.Invoke(router.Target, [session, request]);
        if (result is Task task)
        {
            await task;
        }
    }
}