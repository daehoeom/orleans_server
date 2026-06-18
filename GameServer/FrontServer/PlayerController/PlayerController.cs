using ServerLibrary.Grains;
using ServerLibrary.Models;
using ServerLibrary.Services;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp;

namespace GameServer.PlayerController;

public class PlayerController(IClusterClient clusterClient) 
    : PlayerBaseController(clusterClient)
{
    [PacketPacketHandler(PacketHeaderType.LoadPlayer)]
    public async Task HandleLoadPlayerAsync(PlayerSession session, LoadPlayerReq req)
    {
        var grain = _clusterClient.GetGrain<IPlayerGrain>(session.SessionId);

        
        
        await SendAsync(session, new LoadPlayerRes());
    }
}