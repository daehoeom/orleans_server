using GrainLibrary.Grains;
using GrainLibrary.Models;
using GrainLibrary.Services;
using SharedLibrary;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp.Shop;

namespace GameServer.Controllers;

public class ShopController(IClusterClient clusterClient)
    : PlayerBaseController(clusterClient)
{
    [PacketHandler(PacketHeaderType.Purchase)]
    public async Task PurchaseProductAsync(PlayerSession player, PurchaseProductReq req)
    {
        var shopGrain = _clusterClient.GetGrain<IPlayerShopGrain>(player.SessionId);

        var result = await shopGrain.PurchaseProductAsync(player.SessionId, req.ProductId, req.Count);
        if (result != ResultCode.Success)
        {
            await SendAsync<PurchaseProductRes>(player, result);
            return;
        }

        await SendAsync(player, response: new PurchaseProductRes
        {
            ProductId = req.ProductId,
        });
    }
}