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
    public async Task LoadPlayerAsync(PlayerSession session, LoadPlayerReq req)
    {
        var grain = _clusterClient.GetGrain<IPlayerGrain>(session.SessionId);

        
        
        await SendAsync(session, new LoadPlayerRes());
    }

    [PacketHandler(PacketHeaderType.KeepAlive)]
    public Task KeepAliveAsync(PlayerSession session, KeepAliveReq _)
    {
        return SendAsync(session, new KeepAliveRes()
        {
            DateTimeTicks = TimeUtil.UtcNow.Ticks,
        });
    }
}