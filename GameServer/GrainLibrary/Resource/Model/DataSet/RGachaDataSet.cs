using GrainLibrary.Resource.Model.Row;
using SharedLibrary;

namespace GrainLibrary.Resource.Model.DataSet;

public class RGachaDataSet : IDataSet<RGacha>
{
    private readonly Dictionary<int, RGacha> _gachas = new();

    public bool Load(RGacha data)
    {
        return _gachas.TryAdd(data.GachaId, data);
    }

    public bool PostProcess(RGacha data)
    {
        return data is { GachaId: > 0, CostAmount: > 0 } && data.CostCurrencyType != CurrencyType.None;
    }

    public RGacha? Find(int gachaId)
    {
        return _gachas.GetValueOrDefault(gachaId);
    }
}
