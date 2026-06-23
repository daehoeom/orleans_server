using ServerLibrary.Grains;
using ServerLibrary.Models;
using ServerLibrary.Services;
using ServerLibrary.Utility;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp;

namespace GameServer.Controllers;

public class PlayerController(IClusterClient clusterClient) 
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.LoadPlayer)]
    public async Task LoadPlayerAsync(PlayerSession player, LoadPlayerReq req)
    {
        var playerGrain = _clusterClient.GetGrain<IPlayerGrain>(player.SessionId);

        var data = await playerGrain.GetPlayerData();
        
        await SendAsync(player, new LoadPlayerRes
        {
            PlayerId = data.PlayerId,
        });
    }

    [PacketHandler(PacketHeaderType.KeepAlive)]
    public Task KeepAliveAsync(PlayerSession player, KeepAliveReq _)
    {
        return SendAsync(player, new KeepAliveRes
        {
            DateTimeTicks = TimeUtil.UtcNow.Ticks,
        });
    }
}