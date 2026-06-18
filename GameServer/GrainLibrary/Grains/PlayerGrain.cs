namespace ServerLibrary.Grains;

public interface IPlayerGrain : IGrainWithIntegerKey
{
    Task<long> GetPrimaryKey();
}

public class PlayerGrain : Grain, IPlayerGrain 
{
    public Task<long> GetPrimaryKey()
    {
        return Task.FromResult(this.GetPrimaryKeyLong());
    }
}