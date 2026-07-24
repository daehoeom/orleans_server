namespace GrainLibrary.Resource.Model;

public class RConstants
{
    public int StaminaRecoverIntervalSeconds { get; set; }

    public int StaminaRecoverAmount { get; set; }

    public int StaminaDefaultMaxAmount { get; set; }
    
    public long MaxCurrencyAmount { get; set; } = 2_100_000_000;
    
    public int MaxUnitStack { get; set; } = 99_999;

    public int ResetHour { get; set; } = 11;
}
