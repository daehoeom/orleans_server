namespace SharedLibrary
{
    public enum CurrencyType
    {
        None = 0,
        PaidClover = 1,
        FreeClover = 2,
        Gold = 3,
    
        GachaTicket = 10,
        PremiumGachaTicket = 11,
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

    public enum MailType
    {
        System = 0,
        Reward = 1,
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

    public enum EventType
    {
        None = 0,
        Attendance,
        Roulette,
    }
}