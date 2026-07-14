using GrainLibrary.Resource.Model.Row;

namespace GrainLibrary.Resource.Model.DataSet;

public class RUnitGradeDataSet : IDataSet<RUnitGrade>
{
    private readonly Dictionary<int, RUnitGrade> _grades = new();

    public bool Load(RUnitGrade data)
    {
        return _grades.TryAdd(data.Grade, data);
    }

    public bool PostProcess(RUnitGrade data)
    {
        return data is { Grade: > 0 };
    }

    public RUnitGrade? Find(int grade)
    {
        return _grades.GetValueOrDefault(grade);
    }
}
