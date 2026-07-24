using GrainLibrary.Grains;
using GrainLibrary.Models;
using GrainLibrary.Services;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp.Stage;

namespace GameServer.Controllers;

public class StageController(IClusterClient clusterClient)
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.EnterStage)]
    public async Task EnterStageAsync(PlayerSession player, EnterStageReq req)
    {
        var stageGrain = _clusterClient.GetGrain<IPlayerStageGrain>(player.SessionId);

        var result = await stageGrain.EnterStageAsync(req.StageId);
        if (result.ResultCode != ResultCode.Success)
        {
            await SendAsync<EnterStageRes>(player, result.ResultCode);
            return;
        }

        await SendAsync(player, response: new EnterStageRes
        {
            StageId = req.StageId,
            StaminaModel = result.StaminaInfo!,
        });
    }

    [PacketHandler(PacketHeaderType.ClearStage)]
    public async Task ClearStageAsync(PlayerSession player, ClearStageReq req)
    {
        var stageGrain = _clusterClient.GetGrain<IPlayerStageGrain>(player.SessionId);

        var result = await stageGrain.ClearStageAsync(
            req.StageId, req.Mission1, req.Mission2, req.Mission3, req.ClearScore);
        if (result.ResultCode != ResultCode.Success)
        {
            await SendAsync<ClearStageRes>(player, result.ResultCode);
            return;
        }

        await SendAsync(player, response: new ClearStageRes
        {
            StageInfoModel = result.StageInfo!,
            WalletInfo = result.WalletInfo,
            Level = result.Level,
            Exp = result.Exp,
        });
    }

    [PacketHandler(PacketHeaderType.FailStage)]
    public async Task FailStageAsync(PlayerSession player, FailStageReq req)
    {
        var stageGrain = _clusterClient.GetGrain<IPlayerStageGrain>(player.SessionId);

        var resultCode = await stageGrain.FailStageAsync(req.StageId);

        await SendAsync(player, resultCode, new FailStageRes
        {
            StageId = req.StageId,
        });
    }
}
