using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GrainLibrary.Resource.Attribute;
using GrainLibrary.Resource.Model;
using GrainLibrary.Resource.Model.DataSet;
using Microsoft.Extensions.Logging;

namespace GrainLibrary.Resource;

public class ResourceService(ILogger<ResourceService> logger)
{
    private static readonly string DataRoot = Path.Combine(AppContext.BaseDirectory, "TableData", "Json");

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public RShopProductDataSet ShopProduct { get; private set; } = null!;
    public RLevelDataSet Level { get; private set; } = null!;
    public RUnitDataSet Unit { get; private set; } = null!;
    public RUnitLevelDataSet UnitLevel { get; private set; } = null!;
    public RItemDataSet Item { get; private set; } = null!;
    public RGachaDataSet Gacha { get; private set; } = null!;
    public RGachaUnitDataSet GachaUnit { get; private set; } = null!;
    public RStageDataSet Stage { get; private set; } = null!;
    public RAttendanceDataSet Attendance { get; private set; } = null!;
    public RConstants Constants { get; private set; } = null!;

    public void LoadAll()
    {
        var errors = new List<string>();

        // Constants = LoadConstants(errors);
        // ShopProduct = Load<RShopProductDataSet, RShopProduct>(errors);
        // Level = Load<RLevelDataSet, RLevel>(errors);
        // Unit = Load<RUnitDataSet, RUnit>(errors);
        // UnitLevel = Load<RUnitLevelDataSet, RUnitLevel>(errors);
        // Item = Load<RItemDataSet, RItem>(errors);
        // Gacha = Load<RGachaDataSet, RGacha>(errors);
        // GachaUnit = Load<RGachaUnitDataSet, RGachaUnit>(errors);
        // Stage = Load<RStageDataSet, RStage>(errors);
        // AttendanceReward = Load<RAttendanceDataSet, RAttendanceReward>(errors);

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"[ResourceLoader] 테이블 데이터 로드 실패:\n{string.Join("\n", errors)}");
        }
    }

    private TDataSet Load<TDataSet, TRow>(List<string> errors)
        where TDataSet : IDataSet<TRow>, new()
        where TRow : class
    {
        var dataSet = new TDataSet();

        var tableName = typeof(TRow).GetCustomAttribute<ResourceTableAttribute>()?.TableName;
        if (tableName is null)
        {
            errors.Add($"{typeof(TRow).Name}: ResourceTable 어트리뷰트가 없습니다.");
            return dataSet;
        }

        var filePath = Path.Combine(DataRoot, $"{tableName}.json");
        if (!File.Exists(filePath))
        {
            errors.Add($"{tableName}: 파일을 찾을 수 없습니다. ({filePath})");
            return dataSet;
        }

        var rows = JsonSerializer.Deserialize<List<TRow>>(File.ReadAllText(filePath), SerializerOptions);
        if (rows is null)
        {
            errors.Add($"{tableName}: JSON 파싱 결과가 비어있습니다. ({filePath})");
            return dataSet;
        }

        var loadedRows = new List<TRow>();

        foreach (var row in rows)
        {
            if (row is null)
            {
                continue;
            }

            if (!dataSet.Load(row))
            {
                errors.Add($"{tableName}: Load 실패 - {row}");
                continue;
            }

            loadedRows.Add(row);
        }

        foreach (var row in loadedRows)
        {
            if (!dataSet.PostProcess(row))
            {
                errors.Add($"{tableName}: 검증(PostProcess) 실패 - {row}");
            }
        }

        logger.LogInformation($"[ResourceLoader] {tableName} 로드 완료 ({loadedRows.Count}건)");

        return dataSet;
    }

    private RConstants LoadConstants(List<string> errors)
    {
        var filePath = Path.Combine(DataRoot, "Constants.json");
        if (!File.Exists(filePath))
        {
            errors.Add($"Constants: 파일을 찾을 수 없습니다. ({filePath})");
            return new RConstants();
        }

        var constants = JsonSerializer.Deserialize<RConstants>(File.ReadAllText(filePath), SerializerOptions);
        if (constants is null)
        {
            errors.Add($"Constants: JSON 파싱 결과가 비어있습니다. ({filePath})");
            return new RConstants();
        }

        logger.LogInformation("[ResourceLoader] Constants 로드 완료");

        return constants;
    }
}
