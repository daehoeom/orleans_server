using GrainLibrary.Grains;
using GrainLibrary.Models;
using GrainLibrary.Services;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp.Mail;

namespace GameServer.Controllers;

public class MailController(IClusterClient clusterClient)
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.LoadMail)]
    public async Task LoadMailAsync(PlayerSession player, LoadMailReq req)
    {
        var mailGrain = _clusterClient.GetGrain<IPlayerMailGrain>(player.SessionId);

        var mails = await mailGrain.GetAllAsync();

        await SendAsync(player, response: new LoadMailRes
        {
            Mails = mails,
        });
    }

    [PacketHandler(PacketHeaderType.ReadMail)]
    public async Task ReadMailAsync(PlayerSession player, ReadMailReq req)
    {
        var mailGrain = _clusterClient.GetGrain<IPlayerMailGrain>(player.SessionId);

        var result = await mailGrain.ReadAsync(req.Id);
        if (result.ResultCode != ResultCode.Success)
        {
            await SendAsync<ReadMailRes>(player, result.ResultCode);
            return;
        }

        await SendAsync(player, response: new ReadMailRes
        {
            MailInfo = result.MailInfo!,
        });
    }

    [PacketHandler(PacketHeaderType.ClaimMail)]
    public async Task ClaimMailAsync(PlayerSession player, ClaimMailReq req)
    {
        var mailGrain = _clusterClient.GetGrain<IPlayerMailGrain>(player.SessionId);

        var result = await mailGrain.ClaimAsync(req.Id);
        if (result.ResultCode != ResultCode.Success)
        {
            await SendAsync<ClaimMailRes>(player, result.ResultCode);
            return;
        }

        await SendAsync(player, response: new ClaimMailRes
        {
            MailInfo = result.MailInfo!,
            WalletInfo = result.WalletInfo,
        });
    }

    [PacketHandler(PacketHeaderType.DeleteMail)]
    public async Task DeleteMailAsync(PlayerSession player, DeleteMailReq req)
    {
        var mailGrain = _clusterClient.GetGrain<IPlayerMailGrain>(player.SessionId);

        var resultCode = await mailGrain.DeleteAsync(req.Id);

        await SendAsync(player, resultCode, new DeleteMailRes
        {
            Id = req.Id,
        });
    }
}
