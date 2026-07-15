using GrainLibrary.Grains;
using GrainLibrary.Models;
using GrainLibrary.Services;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp.Gacha;

namespace GameServer.Controllers;

public class GachaController(IClusterClient clusterClient)
    : PlayerBaseController(clusterClient)
{
    private const int ONE_GACHA_COUNT = 1;
    private const int TEN_GACHA_COUNT = 10;
    
    [PacketHandler(PacketHeaderType.RollGacha)]
    public async Task RollGachaAsync(PlayerSession player, RollGachaReq req)
    {
        var gachaGrain = _clusterClient.GetGrain<IPlayerGachaGrain>(player.SessionId);

        if (req.Count != ONE_GACHA_COUNT || req.Count != TEN_GACHA_COUNT)
        {
            await SendAsync<RollGachaRes>(player, ResultCode.InvalidParameter);
            return;
        }
        
        var result = await gachaGrain.RollingGachaAsync(req.GachaId, req.Count);
        if (result.ResultCode != ResultCode.Success)
        {
            await SendAsync<RollGachaRes>(player, result.ResultCode);
            return;
        }

        await SendAsync(player, response: new RollGachaRes
        {
            GachaId = req.GachaId,
            Units = result.Units.Select(unit => new GachaUnitResult
            {
                UnitId = unit.UnitId,
                Level = unit.Level,
                Stack = unit.Stack,
            }).ToList(),
        });
    }
}
