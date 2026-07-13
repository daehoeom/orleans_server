namespace SharedLibrary;

public enum CurrencyType
{
    None = 0,
    PaidClover = 1,
    FreeClover = 2,
    Gold = 3,
}

public enum ChatType
{
    None = 0,
    Channel = 1,
    Whisper = 2,
}

public static class SharedConstant
{
    public static readonly long MAX_CURRENCY_AMOUNT = 2_100_000_000;
}