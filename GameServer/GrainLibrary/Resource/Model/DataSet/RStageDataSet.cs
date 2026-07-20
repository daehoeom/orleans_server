using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class RStageDataSet : IDataSet<RStage>
{
    private readonly Dictionary<int, RStage> _stages = new();

    public bool Load(RStage data)
    {
        return _stages.TryAdd(data.StageId, data);
    }

    public bool PostProcess(RStage data)
    {
        return data is { StageId: > 0, StaminaCost: > 0 };
    }

    public RStage? Find(int stageId)
    {
        return _stages.GetValueOrDefault(stageId);
    }
}
