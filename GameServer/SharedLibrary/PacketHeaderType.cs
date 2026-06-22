namespace SharedLibrary;

public enum PacketHeaderType
{
    None = 0,
    KeepAlive = 1,
    
    // player
    LoadPlayer = 1000,
    
    // Shop
    LoadShop = 2000,
    
}