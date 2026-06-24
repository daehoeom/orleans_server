using GrainLibrary.Models;
using GrainLibrary.Services;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp;

namespace GameServer.Controllers;

public class CommunityController(IClusterClient clusterClient, SessionService sessionService)
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.SendChat)]
    public async Task SendChatAsync(PlayerSession player, SendChatReq request)
    {
        // 대화내용 필터 추가

        switch (request.ChatType)
        {
            case ChatType.Channel:
            {
                var ntf = new ChatNtf { Message = request.Message };
                sessionService.Broadcast(ntf);
                break;
            }

            case ChatType.Whisper:
            {
                var session = sessionService.GetSession(request.TargetUser);
                if (session == null)
                {
                    await SendAsync<SendChatRes>(player, ResultCode.PlayerNotFound);
                    return;
                }

                var ntf = new ChatNtf { Message = request.Message };
                session.Notify(ntf);
                break;
            }
        }

        await SendAsync(player, response: new SendChatRes());
    }
}