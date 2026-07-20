using Database.Redis;
using Database.Redis.RedisSet;
using GrainLibrary.Models;
using GrainLibrary.Services;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp;

namespace GameServer.Controllers;

public class AuthController(IClusterClient clusterClient, RedisService redisService, SessionService sessionService)
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.Auth)]
    public async Task AuthAsync(PlayerSession player, AuthPlayerReq req)
    {
        if (string.IsNullOrEmpty(req.AccessToken))
        {
            await SendAsync<AuthPlayerRes>(player, ResultCode.AuthTokenInvalid);
            return;
        }

        var tokenSet = new PlayerAccessTokenSet(redisService.GetDatabase(0));
        var storedPlayerId = await tokenSet.StringGet(req.AccessToken);
        if (string.IsNullOrEmpty(storedPlayerId) || !long.TryParse(storedPlayerId, out var playerId) || playerId != req.AccountId)
        {
            await SendAsync<AuthPlayerRes>(player, ResultCode.AuthTokenInvalid);
            return;
        }

        player.CompleteAuth(playerId);
        sessionService.AddSession(player);

        await SendAsync(player, response: new AuthPlayerRes
        {
            PlayerId = playerId,
        });
    }
}
