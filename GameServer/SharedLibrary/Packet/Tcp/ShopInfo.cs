using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    public class ShopInfo 
    {
    }
    
    [MessagePackObject]
    [Response(PacketHeaderType.LoadShop)]
    public class ShopInfoRes 
    {
    }
}

