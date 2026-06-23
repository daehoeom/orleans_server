using ServerLibrary.Models;
using ServerLibrary.Services;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp;

namespace GameServer.Controllers;

public class CommunityController(IClusterClient clusterClient, SessionService sessionService)
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.SendChat)]
    public Task SendChatAsync(PlayerSession player, SendChatReq request)
    {
        // 대화내용 필터 추가

        var ntf = new ChatNtf()
        {
            Message = request.Message,
        };
        sessionService.Broadcast(ntf);

        return SendAsync(player, new SendChatRes());
    }
}