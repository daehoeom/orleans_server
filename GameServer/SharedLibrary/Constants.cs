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

public enum ItemType
{
    None = 0,
    
}

public enum UnitGradeType
{
    None = 0,
    UnCommon,
    Common,
    Rare,
    Unique,
    A,
    S,
    SR,
    SSR,
}

public static class SharedConstant
{
    public static readonly long MAX_CURRENCY_AMOUNT = 2_100_000_000;
}