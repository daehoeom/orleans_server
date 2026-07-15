using Database.Db;
using Database.Db.Row;
using GrainLibrary.Grains.Dto;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerStageGrain : IGrainWithIntegerKey
{
    Task<StageStateDto?> GetAsync(int stageIndex);
    Task<IReadOnlyList<StageStateDto>> GetAllAsync();
    Task<ResultCode> ClearStageAsync(int stageIndex, bool missionStep1, bool missionStep2, bool missionStep3, short clearScore);
}

public class PlayerStageGrain(DatabaseService dbService) : Grain, IPlayerStageGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    // 활성화 시 DB Row를 DTO로 변환해 캐싱하고, 이후에는 ClearStage에서 DB와 함께 write-through로 갱신한다.
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

    public async Task<ResultCode> ClearStageAsync(
        int stageIndex, bool missionStep1, bool missionStep2, bool missionStep3, short clearScore)
    {
        if (_stages.TryGetValue(stageIndex, out var stage))
        {
            var affectedRow = await dbService.Game.StageStates.UpdateAsync(
                PlayerId, stageIndex, missionStep1, missionStep2, missionStep3, clearScore);
            if (affectedRow <= 0)
            {
                return ResultCode.DbUpdateError;
            }

            stage.MissionStep1 = missionStep1;
            stage.MissionStep2 = missionStep2;
            stage.MissionStep3 = missionStep3;
            stage.ClearScore = clearScore;

            return ResultCode.Success;
        }

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
            return ResultCode.DbInsertError;
        }

        _stages[stageIndex] = new StageStateDto
        {
            StageIndex = stageIndex,
            MissionStep1 = missionStep1,
            MissionStep2 = missionStep2,
            MissionStep3 = missionStep3,
            ClearScore = clearScore,
        };

        return ResultCode.Success;
    }
}
