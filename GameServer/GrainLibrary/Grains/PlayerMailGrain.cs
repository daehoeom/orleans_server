using System.Text.Json;
using Database.Db;
using GrainLibrary.Grains.Dto;
using GrainLibrary.Utility;
using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains;

public interface IPlayerMailGrain : IGrainWithIntegerKey
{
    Task<List<MailModel>> GetAllAsync();
    Task<MailReadResultDto> ReadAsync(long id);
    Task<ResultCode> DeleteAsync(long id);
    Task<ResultCode> DeleteAllAsync();
}

public class PlayerMailGrain(DatabaseService dbService) : Grain, IPlayerMailGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    private static readonly List<MailRewardEntry> EmptyRewards = new();

    private readonly Dictionary<long, MailModel> _mails = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var rows = await dbService.Game.Mails.GetsAsync(PlayerId);
        foreach (var row in rows)
        {
            _mails[row.id] = new MailModel
            {
                Id = row.id,
                MailId = row.mail_id,
                Type = (MailType)row.type,
                Title = row.title,
                Body = row.body,
                Rewards = JsonSerializer.Deserialize<List<MailRewardEntry>>(row.rewards) ?? new List<MailRewardEntry>(),
                IsRead = row.is_read,
                ExpiredAt = row.expired_at,
            };
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<List<MailModel>> GetAllAsync()
    {
        var now = TimeUtil.UtcNow;
        var result = _mails.Values.Where(mail => mail.ExpiredAt > now).ToList();

        return Task.FromResult(result);
    }

    // 메일에 보상이 남아있으면 '읽음'은 곧 '수령'을 의미한다 - 보상이 없으면 단순 읽음 처리만 한다.
    public async Task<MailReadResultDto> ReadAsync(long id)
    {
        if (!_mails.TryGetValue(id, out var mail))
        {
            return new MailReadResultDto { ResultCode = ResultCode.MailNotFound };
        }

        if (mail.Rewards.Count == 0)
        {
            if (!mail.IsRead)
            {
                var affectedRow = await dbService.Game.Mails.UpdateReadAsync(PlayerId, id);
                if (affectedRow <= 0)
                {
                    return new MailReadResultDto { ResultCode = ResultCode.DbUpdateError };
                }

                mail.IsRead = true;
            }

            return new MailReadResultDto
            {
                ResultCode = ResultCode.Success,
                MailInfo = mail,
            };
        }

        var walletGrain = GrainFactory.GetGrain<IPlayerWalletGrain>(PlayerId);

        var rewardGrant = await RewardHelper.GrantAsync(
            GrainFactory, PlayerId,
            mail.Rewards.Select(reward => (reward.CurrencyType, reward.CurrencyAmount)),
            mail.Rewards.Select(reward => (reward.ItemId, reward.ItemCount)));

        var rewardAffectedRow = await dbService.Game.Mails.UpdateRewardsAsync(PlayerId, id, JsonSerializer.Serialize(EmptyRewards));
        if (rewardAffectedRow <= 0)
        {
            return new MailReadResultDto { ResultCode = ResultCode.DbUpdateError };
        }

        var grantedRewards = mail.Rewards;

        mail.Rewards = new List<MailRewardEntry>();
        mail.IsRead = true;

        return new MailReadResultDto
        {
            ResultCode = ResultCode.Success,
            MailInfo = new MailModel
            {
                Id = mail.Id,
                MailId = mail.MailId,
                Type = mail.Type,
                Title = mail.Title,
                Body = mail.Body,
                Rewards = grantedRewards,
                IsRead = mail.IsRead,
                ExpiredAt = mail.ExpiredAt,
            },
            WalletInfo = await walletGrain.GetAllBalanceAsync(),
            RewardGrant = rewardGrant,
        };
    }

    public async Task<ResultCode> DeleteAsync(long id)
    {
        if (!_mails.TryGetValue(id, out var mail))
        {
            return ResultCode.MailNotFound;
        }

        if (mail.Rewards.Count > 0)
        {
            return ResultCode.MailRewardNotClaimed;
        }

        var affectedRow = await dbService.Game.Mails.DeleteAsync(PlayerId, id);
        if (affectedRow <= 0)
        {
            return ResultCode.DbUpdateError;
        }

        _mails.Remove(id);

        return ResultCode.Success;
    }

    public async Task<ResultCode> DeleteAllAsync()
    {
        if (!_mails.Any(p => p.Value.IsRead))
        {
            return ResultCode.Success;
        }

        _ = await dbService.Game.Mails.DeleteAllAsync(PlayerId);

        foreach (var mail in _mails)
        {
            if (mail.Value.IsRead)
            {
                _mails.Remove(mail.Key);
            }
        }

        return ResultCode.Success;
    }
}
