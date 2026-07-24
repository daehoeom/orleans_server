using Database.Db;
using Database.Db.Row;
using GrainLibrary.Resource;
using GrainLibrary.Utility;
using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains;

public interface IPlayerStaminaGrain : IGrainWithIntegerKey
{
    Task<StaminaModel> GetAsync();
    Task<ResultCode> ConsumeAsync(int amount);
}

public class PlayerStaminaGrain(DatabaseService dbService, ResourceService resourceService) : Grain, IPlayerStaminaGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    private int _amount;
    private int _maxAmount;
    private DateTime _lastUpdatedAt;
    private bool _exists;

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var row = await dbService.Game.Stamina.GetAsync(PlayerId);
        if (row != null)
        {
            _amount = row.amount;
            _maxAmount = row.max_amount;
            _lastUpdatedAt = row.last_updated_at;
            _exists = true;
        }
        else
        {
            _maxAmount = resourceService.Constants.StaminaDefaultMaxAmount;
            _amount = _maxAmount;
            _lastUpdatedAt = TimeUtil.UtcNow;
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<StaminaModel> GetAsync()
    {
        var amount = CalculateRecoveredAmount(out _);

        return Task.FromResult(new StaminaModel
        {
            Amount = amount,
            MaxAmount = _maxAmount,
        });
    }

    public async Task<ResultCode> ConsumeAsync(int amount)
    {
        if (amount <= 0)
        {
            return ResultCode.InvalidParameter;
        }

        var recoveredAmount = CalculateRecoveredAmount(out var lastUpdatedAt);
        if (recoveredAmount < amount)
        {
            return ResultCode.NotEnoughStamina;
        }

        var remainAmount = recoveredAmount - amount;

        if (!_exists)
        {
            var insertedRow = await dbService.Game.Stamina.InsertAsync(new PlayerStaminaRow
            {
                player_id = PlayerId,
                amount = remainAmount,
                max_amount = _maxAmount,
            });
            if (insertedRow <= 0)
            {
                return ResultCode.DbInsertError;
            }

            _exists = true;
            _amount = remainAmount;
            _lastUpdatedAt = TimeUtil.UtcNow;

            return ResultCode.Success;
        }

        var affectedRow = await dbService.Game.Stamina.UpdateAsync(PlayerId, remainAmount, lastUpdatedAt);
        if (affectedRow <= 0)
        {
            return ResultCode.DbUpdateError;
        }

        _amount = remainAmount;
        _lastUpdatedAt = lastUpdatedAt;

        return ResultCode.Success;
    }

    private int CalculateRecoveredAmount(out DateTime lastUpdatedAt)
    {
        lastUpdatedAt = _lastUpdatedAt;

        if (_amount >= _maxAmount)
        {
            return _amount;
        }

        var intervalSeconds = resourceService.Constants.StaminaRecoverIntervalSeconds;
        if (intervalSeconds <= 0)
        {
            return _amount;
        }

        var elapsedSeconds = (TimeUtil.UtcNow - _lastUpdatedAt).TotalSeconds;
        var intervals = (long)(elapsedSeconds / intervalSeconds);
        if (intervals <= 0)
        {
            return _amount;
        }

        var recoverAmount = resourceService.Constants.StaminaRecoverAmount;
        var recoveredAmount = (int)Math.Min(_amount + intervals * recoverAmount, _maxAmount);

        lastUpdatedAt = recoveredAmount >= _maxAmount
            ? TimeUtil.UtcNow
            : _lastUpdatedAt.AddSeconds(intervals * intervalSeconds);

        return recoveredAmount;
    }
}
