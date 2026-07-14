using Database.Db;
using GrainLibrary.Grains.Dto;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerWalletGrain : IGrainWithIntegerKey
{
    Task<long> GetBalanceAsync(CurrencyType currencyType);
    Task<ResultCode> SpendAsync(CurrencyType currencyType, long amount);
    Task<long> AddAsync(CurrencyType currencyType, long amount);
    Task<ResultCode> IsEnoughAsync(CurrencyType currencyType, long amount);
}

public class PlayerWalletGrain(DatabaseService dbService) : Grain, IPlayerWalletGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    // 활성화 시 DB Row를 DTO로 변환해 캐싱하고, 이후에는 Spend/Add에서 DB와 함께 write-through로 갱신한다.
    private readonly Dictionary<CurrencyType, WalletDto> _wallets = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var wallets = await dbService.Game.Wallets.GetsAsync(PlayerId);
        foreach (var wallet in wallets)
        {
            var currencyType = (CurrencyType)wallet.monetary_type;
            _wallets[currencyType] = new WalletDto
            {
                CurrencyType = currencyType,
                Amount = wallet.amount,
            };
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<long> GetBalanceAsync(CurrencyType currencyType)
    {
        return Task.FromResult(_wallets.GetValueOrDefault(currencyType)?.Amount ?? 0);
    }

    public async Task<ResultCode> SpendAsync(CurrencyType currencyType, long amount)
    {
        var balance = _wallets.GetValueOrDefault(currencyType)?.Amount ?? 0;
        if (balance < amount)
        {
            return ResultCode.NotEnoughCurrency;
        }

        await dbService.Game.Wallets.SpendAsync(PlayerId, (int)currencyType, amount);

        SetBalance(currencyType, balance - amount);

        return ResultCode.Success;
    }

    public async Task<long> AddAsync(CurrencyType currencyType, long amount)
    {
        var balance = _wallets.GetValueOrDefault(currencyType)?.Amount ?? 0;

        var addAmount = Math.Min(amount, SharedConstant.MAX_CURRENCY_AMOUNT - balance);
        if (addAmount <= 0)
        {
            return balance;
        }

        await dbService.Game.Wallets.AddAsync(PlayerId, (int)currencyType, addAmount);

        balance += addAmount;
        SetBalance(currencyType, balance);

        return balance;
    }

    public Task<ResultCode> IsEnoughAsync(CurrencyType currencyType, long amount)
    {
        if (!_wallets.TryGetValue(currencyType, out var wallet) || wallet.Amount < amount)
        {
            return Task.FromResult(ResultCode.NotEnoughCurrency);
        }

        return Task.FromResult(ResultCode.Success);
    }

    private void SetBalance(CurrencyType currencyType, long amount)
    {
        if (_wallets.TryGetValue(currencyType, out var wallet))
        {
            wallet.Amount = amount;
            return;
        }

        _wallets[currencyType] = new WalletDto
        {
            CurrencyType = currencyType,
            Amount = amount,
        };
    }
}
