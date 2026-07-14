using SharedLibrary;

namespace GrainLibrary.Grains.Dto;

public class WalletDto
{
    public CurrencyType CurrencyType { get; set; }

    public long Amount { get; set; }
}
