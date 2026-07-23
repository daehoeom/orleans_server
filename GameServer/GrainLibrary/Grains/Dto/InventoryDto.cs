using SharedLibrary;

namespace GrainLibrary.Grains.Dto;

public class InventoryDto
{
    public int ItemId { get; set; }

    public int Count { get; set; }
}

public class ItemAddResult
{
    public int Requested;
    public int Granted;
    public int NewCount;
    public ResultCode ResultCode;
}