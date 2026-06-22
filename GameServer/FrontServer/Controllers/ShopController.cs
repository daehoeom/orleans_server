using ServerLibrary.Services;

namespace GameServer.Controllers;

public class ShopController(IClusterClient clusterClient)
    : PlayerBaseController(clusterClient)
{
    
}