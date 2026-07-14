using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GrainLibrary.Resource.Attribute;
using GrainLibrary.Resource.Model;
using GrainLibrary.Resource.Model.DataSet;
using GrainLibrary.Resource.Model.Row;
using Microsoft.Extensions.Logging;

namespace GrainLibrary.Resource;

public class ResourceLoader(ILogger<ResourceLoader> logger)
{
    private static readonly string DataRoot = Path.Combine(AppContext.BaseDirectory, "TableData", "Json");

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public ShopProductDataSet ShopProduct { get; private set; } = null!;
    public LevelDataSet Level { get; private set; } = null!;

    public void LoadAll()
    {
        var errors = new List<string>();

        // 테이블을 추가할 때마다 이곳에 명시적으로 등록한다.
        ShopProduct = Load<ShopProductDataSet, RShopProduct>(errors);
        Level = Load<LevelDataSet, RLevel>(errors);

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
}
