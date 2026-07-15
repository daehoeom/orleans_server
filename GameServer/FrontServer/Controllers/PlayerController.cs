using GrainLibrary.Grains;
using GrainLibrary.Models;
using GrainLibrary.Services;
using GrainLibrary.Utility;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp;

namespace GameServer.Controllers;

public class PlayerController(IClusterClient clusterClient) 
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.LoadPlayer)]
    public async Task LoadPlayerAsync(PlayerSession player, LoadPlayerReq _)
    {
        var playerGrain = _clusterClient.GetGrain<IPlayerGrain>(player.SessionId);

        var data = await playerGrain.GetPlayerData();

        var walletGrain = _clusterClient.GetGrain<IPlayerWalletGrain>(player.SessionId);
        var walletInfo = await walletGrain.GetAllBalanceAsync();

        var unitGrain = _clusterClient.GetGrain<IPlayerUnitGrain>(player.SessionId);
        var unitInfo = await unitGrain.GetAllInfoAsync();
        
        await SendAsync(player, response: new LoadPlayerRes
        {
            PlayerId = data.PlayerId,
            WalletInfo = walletInfo,
            UnitInfo = unitInfo,
        });
    }

    [PacketHandler(PacketHeaderType.KeepAlive)]
    public Task KeepAliveAsync(PlayerSession player, KeepAliveReq _)
    {
        return SendAsync(player, response: new KeepAliveRes
        {
            DateTimeTicks = TimeUtil.UtcNow.Ticks,
        });
    }
}