namespace SharedLibrary
{
    public enum PacketHeaderType
    {
        None = 0,
        KeepAlive = 1,
        Auth = 2,
    
        // player
        LoadPlayer = 1000,
    
        // Shop
        LoadShop = 2000,
        Purchase = 2001,
    
        // Community
        SendChat = 3000,
        ChatNtf = 3001,

        // Gacha
        RollGacha = 4000,

        // Stage
        EnterStage = 5000,
        ClearStage = 5001,
        FailStage = 5002,

        // Attendance
        LoadAttendance = 6000,
        ReceiveAttendanceReward = 6001,

        // Mail
        LoadMail = 7000,
        ReadMail = 7001,
        DeleteMail = 7003,
    }    
}