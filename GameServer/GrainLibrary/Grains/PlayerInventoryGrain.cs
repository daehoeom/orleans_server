using Database.Db;
using Database.Db.Row;
using GrainLibrary.Grains.Dto;
using GrainLibrary.Resource;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerInventoryGrain : IGrainWithIntegerKey
{
    Task<int> GetCountAsync(int itemId);
    Task<IReadOnlyList<InventoryDto>> GetAllAsync();
    Task<ItemAddResult> AddAsync(int itemId, int count);
    Task<ResultCode> SpendAsync(int itemId, int count);
}

public class PlayerInventoryGrain(DatabaseService dbService, ResourceService resourceService) 
    : Grain, IPlayerInventoryGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

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

    public async Task<ItemAddResult> AddAsync(int itemId, int count)
    {
        if (count <= 0)
        {
            return new ItemAddResult { Requested = count, ResultCode = ResultCode.InvalidParameter };
        }

        var currentCount = _items.GetValueOrDefault(itemId)?.Count ?? 0;

        var rItem = resourceService.Item.Find(itemId);
        var maxStack = rItem?.MaxStack ?? 0;
        var room = maxStack > 0 ? maxStack - currentCount : count;
        var addCount = Math.Max(0, Math.Min(count, room));

        if (addCount == 0)
        {
            return new ItemAddResult { Requested = count, Granted = 0, NewCount = currentCount, ResultCode = ResultCode.Success };
        }

        if (_items.TryGetValue(itemId, out var item))
        {
            var affectedRow = await dbService.Game.Inventory.AddAsync(PlayerId, itemId, addCount);
            if (affectedRow <= 0)
            {
                return new ItemAddResult { Requested = count, ResultCode = ResultCode.DbUpdateError };
            }

            item.Count += addCount;

            return new ItemAddResult { Requested = count, Granted = addCount, NewCount = item.Count, ResultCode = ResultCode.Success };
        }

        var insertedRow = await dbService.Game.Inventory.InsertAsync(new PlayerInventoryRow
        {
            player_id = PlayerId,
            item_id = itemId,
            count = addCount,
        });
        if (insertedRow <= 0)
        {
            return new ItemAddResult { Requested = count, ResultCode = ResultCode.DbInsertError };
        }

        _items[itemId] = new InventoryDto { ItemId = itemId, Count = addCount };

        return new ItemAddResult { Requested = count, Granted = addCount, NewCount = addCount, ResultCode = ResultCode.Success };
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
