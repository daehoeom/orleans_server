using System.Collections.Generic;
using MessagePack;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Data;

namespace SharedLibrary.Packet.Tcp.Shop
{
    [MessagePackObject]
    public class ShopInfo 
    {
    }
    
    [MessagePackObject]
    [Response(PacketHeaderType.LoadShop)]
    public class ShopInfoRes
    {
        [Key(0)] public List<ProductInfo> ProductInfo = new();
    }
}

