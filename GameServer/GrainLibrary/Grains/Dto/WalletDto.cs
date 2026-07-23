using SharedLibrary;

namespace GrainLibrary.Grains.Dto;

public class WalletDto
{
    public CurrencyType CurrencyType { get; set; }

    public long Amount { get; set; }
}

public class WalletAddResult
{
    public long Requested;
    public long Granted;
    public long NewBalance;
    public ResultCode ResultCode;
}