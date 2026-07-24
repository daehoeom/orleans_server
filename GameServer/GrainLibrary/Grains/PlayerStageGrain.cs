using Database.Db;
using Database.Db.Row;
using GrainLibrary.Grains.Dto;
using GrainLibrary.Resource;
using GrainLibrary.Utility;
using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains;

public interface IPlayerStageGrain : IGrainWithIntegerKey
{
    Task<StageStateDto?> GetAsync(int stageIndex);
    Task<IReadOnlyList<StageStateDto>> GetAllAsync();
    Task<List<StageInfo>> GetAllInfoAsync();
    Task<StageEnterResultDto> EnterStageAsync(int stageIndex);
    Task<StageClearResultDto> ClearStageAsync(int stageIndex, bool missionStep1, bool missionStep2, bool missionStep3, short clearScore);
    Task<ResultCode> FailStageAsync(int stageIndex);
}

public class PlayerStageGrain(DatabaseService dbService, ResourceService resourceService) : Grain, IPlayerStageGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    private readonly Dictionary<int, StageStateDto> _stages = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var stages = await dbService.Game.StageStates.GetsAsync(PlayerId);
        foreach (var stage in stages)
        {
            _stages[stage.stage_index] = new StageStateDto
            {
                StageIndex = stage.stage_index,
                MissionStep1 = stage.mission_step_1,
                MissionStep2 = stage.mission_step_2,
                MissionStep3 = stage.mission_step_3,
                ClearScore = stage.clear_score,
            };
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<StageStateDto?> GetAsync(int stageIndex)
    {
        return Task.FromResult(_stages.GetValueOrDefault(stageIndex));
    }

    public Task<IReadOnlyList<StageStateDto>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<StageStateDto>>(_stages.Values.ToList());
    }

    public Task<List<StageInfo>> GetAllInfoAsync()
    {
        var result = _stages.Select(p => new StageInfo
        {
            StageId = p.Value.StageIndex,
            Mission1 = p.Value.MissionStep1,
            Mission2 = p.Value.MissionStep2,
            Mission3 = p.Value.MissionStep3,
            ClearScore = p.Value.ClearScore,
        }).ToList();

        return Task.FromResult(result);
    }

    public async Task<StageEnterResultDto> EnterStageAsync(int stageIndex)
    {
        var rStage = resourceService.Stage.Find(stageIndex);
        if (rStage is null)
        {
            return new StageEnterResultDto { ResultCode = ResultCode.StageNotFound };
        }

        if (stageIndex > 1 && !_stages.ContainsKey(stageIndex - 1))
        {
            return new StageEnterResultDto { ResultCode = ResultCode.StageLocked };
        }

        var staminaGrain = GrainFactory.GetGrain<IPlayerStaminaGrain>(PlayerId);

        var consumeResult = await staminaGrain.ConsumeAsync(rStage.StaminaCost);
        if (consumeResult != ResultCode.Success)
        {
            return new StageEnterResultDto { ResultCode = consumeResult };
        }

        var staminaInfo = await staminaGrain.GetAsync();

        return new StageEnterResultDto
        {
            ResultCode = ResultCode.Success,
            StaminaInfo = staminaInfo,
        };
    }

    public async Task<StageClearResultDto> ClearStageAsync(
        int stageIndex, bool missionStep1, bool missionStep2, bool missionStep3, short clearScore)
    {
        var rStage = resourceService.Stage.Find(stageIndex);
        if (rStage is null)
        {
            return new StageClearResultDto { ResultCode = ResultCode.StageNotFound };
        }

        if (_stages.TryGetValue(stageIndex, out var stage))
        {
            // 이미 달성한 미션/최고 점수는 재도전으로 인해 낮아지지 않도록 병합한다.
            missionStep1 = stage.MissionStep1 || missionStep1;
            missionStep2 = stage.MissionStep2 || missionStep2;
            missionStep3 = stage.MissionStep3 || missionStep3;
            clearScore = Math.Max(stage.ClearScore, clearScore);

            var affectedRow = await dbService.Game.StageStates.UpdateAsync(
                PlayerId, stageIndex, missionStep1, missionStep2, missionStep3, clearScore);
            if (affectedRow <= 0)
            {
                return new StageClearResultDto { ResultCode = ResultCode.DbUpdateError };
            }

            stage.MissionStep1 = missionStep1;
            stage.MissionStep2 = missionStep2;
            stage.MissionStep3 = missionStep3;
            stage.ClearScore = clearScore;
        }
        else
        {
            var insertedRow = await dbService.Game.StageStates.InsertAsync(new PlayerStageStateRow
            {
                player_id = PlayerId,
                stage_index = stageIndex,
                mission_step_1 = missionStep1,
                mission_step_2 = missionStep2,
                mission_step_3 = missionStep3,
                clear_score = clearScore,
            });
            if (insertedRow <= 0)
            {
                return new StageClearResultDto { ResultCode = ResultCode.DbInsertError };
            }

            _stages[stageIndex] = new StageStateDto
            {
                StageIndex = stageIndex,
                MissionStep1 = missionStep1,
                MissionStep2 = missionStep2,
                MissionStep3 = missionStep3,
                ClearScore = clearScore,
            };
        }

        var levelGrain = GrainFactory.GetGrain<IPlayerLevelGrain>(PlayerId);
        if (rStage.RewardExp > 0)
        {
            await levelGrain.AddExpAsync(rStage.RewardExp);
        }

        var walletGrain = GrainFactory.GetGrain<IPlayerWalletGrain>(PlayerId);

        var rewardGrant = await RewardHelper.GrantAsync(
            GrainFactory, PlayerId,
            [(rStage.RewardCurrencyType, rStage.RewardCurrencyAmount)],
            []);

        return new StageClearResultDto
        {
            ResultCode = ResultCode.Success,
            StageInfo = new StageInfo
            {
                StageId = stageIndex,
                Mission1 = missionStep1,
                Mission2 = missionStep2,
                Mission3 = missionStep3,
                ClearScore = clearScore,
            },
            WalletInfo = await walletGrain.GetAllBalanceAsync(),
            Level = await levelGrain.GetLevelAsync(),
            Exp = await levelGrain.GetExpAsync(),
            RewardGrant = rewardGrant,
        };
    }

    public Task<ResultCode> FailStageAsync(int stageIndex)
    {
        var rStage = resourceService.Stage.Find(stageIndex);
        if (rStage is null)
        {
            return Task.FromResult(ResultCode.StageNotFound);
        }

        return Task.FromResult(ResultCode.Success);
    }
}
