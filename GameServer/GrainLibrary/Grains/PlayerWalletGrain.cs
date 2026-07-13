using Database.Db;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerWalletGrain : IGrainWithIntegerKey
{
    Task<long> GetBalanceAsync(CurrencyType currencyType);
    Task<ResultCode> SpendAsync(CurrencyType currencyType, long amount);
    Task<long> AddAsync(CurrencyType currencyType, long amount);
    Task<ResultCode> IsEnough(CurrencyType currencyType, long amount);
}

public class PlayerWalletGrain(DatabaseService dbService) : Grain, IPlayerWalletGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    // 활성화 시 DB에서 한 번만 읽어 캐싱하고, 이후에는 Spend/Add에서 DB와 함께 write-through로 갱신한다.
    private readonly Dictionary<CurrencyType, long> _balances = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var wallets = await dbService.Game.Wallets.GetsAsync(PlayerId);
        foreach (var wallet in wallets)
        {
            _balances[(CurrencyType)wallet.monetary_type] = wallet.amount;
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<long> GetBalanceAsync(CurrencyType currencyType)
    {
        return Task.FromResult(_balances.GetValueOrDefault(currencyType));
    }
    
    public async Task<ResultCode> SpendAsync(CurrencyType currencyType, long amount)
    {
        var balance = _balances.GetValueOrDefault(currencyType);
        if (balance < amount)
        {
            return ResultCode.NotEnoughCurrency;
        }

        await dbService.Game.Wallets.SpendAsync(PlayerId, (int)currencyType, amount);

        balance -= amount;
        _balances[currencyType] = balance;

        return ResultCode.Success;
    }

    public async Task<long> AddAsync(CurrencyType currencyType, long amount)
    {
        var balance = _balances.GetValueOrDefault(currencyType);

        var addAmount = Math.Min(amount, SharedConstant.MAX_CURRENCY_AMOUNT - balance);
        if (addAmount <= 0)
        {
            return balance;
        }

        await dbService.Game.Wallets.AddAsync(PlayerId, (int)currencyType, addAmount);

        balance += addAmount;
        _balances[currencyType] = balance;

        return balance;
    }

    public Task<ResultCode> IsEnough(CurrencyType currencyType, long amount)
    {
        if (!_balances.TryGetValue(currencyType, out var balance) 
            || balance < amount)
        {
            return Task.FromResult(ResultCode.NotEnoughCurrency);
        }

        return Task.FromResult(ResultCode.Success);
    }
}
