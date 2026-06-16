namespace ServerLibrary.Grains;

public interface IPlayerGrain : IGrainWithStringKey
{
    Task<string> GetPrimaryKey();
}

public class PlayerGrain : Grain, IPlayerGrain 
{
    public Task<string> GetPrimaryKey()
    {
        return Task.FromResult(this.GetPrimaryKeyString());
    }
}