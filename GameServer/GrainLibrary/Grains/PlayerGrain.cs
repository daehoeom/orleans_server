using ServerLibrary.Models;

namespace ServerLibrary.Grains;

public interface IPlayerGrain : IGrainWithIntegerKey
{
    Task<long> GetPrimaryKey();

    Task<PlayerData> GetPlayerData();
}

public class PlayerGrain : Grain, IPlayerGrain
{
    private long _playerId;
    
    public Task<long> GetPrimaryKey()
    {
        return Task.FromResult(this.GetPrimaryKeyLong());
    }

    public Task<PlayerData> GetPlayerData()
    {
        return Task.FromResult(new PlayerData()
        {
            PlayerId = _playerId,
        });
    }
}