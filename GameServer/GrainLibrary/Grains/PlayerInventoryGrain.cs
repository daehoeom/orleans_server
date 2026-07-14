using Database.Db;
using Database.Db.Row;
using GrainLibrary.Grains.Dto;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerInventoryGrain : IGrainWithIntegerKey
{
    Task<int> GetCountAsync(int itemId);
    Task<IReadOnlyList<InventoryDto>> GetAllAsync();
    Task<ResultCode> AddAsync(int itemId, int count);
    Task<ResultCode> SpendAsync(int itemId, int count);
}

public class PlayerInventoryGrain(DatabaseService dbService) : Grain, IPlayerInventoryGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    // 활성화 시 DB Row를 DTO로 변환해 캐싱하고, 이후에는 Add/Spend에서 DB와 함께 write-through로 갱신한다.
    private readonly Dictionary<int, InventoryDto> _items = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var items = await dbService.Game.Inventory.GetsAsync(PlayerId);
        foreach (var item in items)
        {
            _items[item.item_id] = new InventoryDto
            {
                ItemId = item.item_id,
                Count = item.count,
            };
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<int> GetCountAsync(int itemId)
    {
        return Task.FromResult(_items.GetValueOrDefault(itemId)?.Count ?? 0);
    }

    public Task<IReadOnlyList<InventoryDto>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<InventoryDto>>(_items.Values.ToList());
    }

    public async Task<ResultCode> AddAsync(int itemId, int count)
    {
        if (count <= 0)
        {
            return ResultCode.InvalidParameter;
        }

        if (_items.TryGetValue(itemId, out var item))
        {
            var affectedRow = await dbService.Game.Inventory.AddAsync(PlayerId, itemId, count);
            if (affectedRow <= 0)
            {
                return ResultCode.DbUpdateError;
            }

            item.Count += count;

            return ResultCode.Success;
        }

        var insertedRow = await dbService.Game.Inventory.InsertAsync(new PlayerInventoryRow
        {
            player_id = PlayerId,
            item_id = itemId,
            count = count,
        });
        if (insertedRow <= 0)
        {
            return ResultCode.DbInsertError;
        }

        _items[itemId] = new InventoryDto
        {
            ItemId = itemId,
            Count = count,
        };

        return ResultCode.Success;
    }

    public async Task<ResultCode> SpendAsync(int itemId, int count)
    {
        if (count <= 0)
        {
            return ResultCode.InvalidParameter;
        }

        if (!_items.TryGetValue(itemId, out var item) || item.Count < count)
        {
            return ResultCode.NotEnoughItemCount;
        }

        var affectedRow = await dbService.Game.Inventory.SpendAsync(PlayerId, itemId, count);
        if (affectedRow <= 0)
        {
            return ResultCode.DbUpdateError;
        }

        item.Count -= count;

        return ResultCode.Success;
    }
}
