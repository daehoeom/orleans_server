using ScenarioBot.Session;
using SharedLibrary;
using SharedLibrary.Packet.Tcp.Shop;

namespace ScenarioBot.Scenario;

public class ShopPurchaseScenario : IScenario
{
    public string Name => "ShopPurchase";

    public async Task RunAsync(BotClientSession session)
    {
        await session.SendAsync(PacketHeaderType.Purchase, new PurchaseProductReq
        {
            ProductId = 1001,
            Count = 1,
        });

        var purchaseRes = await session.WaitForResponseAsync<PurchaseProductRes>();
        if (purchaseRes.ResultCode != ResultCode.Success)
        {
            throw new Exception($"[{Name}] 실패: {purchaseRes.ResultCode}");
        }

        Console.WriteLine($"[{Name}] 구매 성공: ProductId={purchaseRes.Stream.ProductId}");
    }
}
