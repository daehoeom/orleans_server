namespace SharedLibrary
{
    public enum ResultCode
    {
        Success = 0,

        #region System Error

        InternalServeError = -1000,
        NotFoundResource,
        InvalidParameter,
        DbInsertError,
        DbUpdateError,
        Unauthorized,
        

        #endregion

        #region Player Error

        PlayerNotFound = -2000,
        NotEnoughStamina,
        AuthTokenInvalid,
        
        #endregion

        #region Shop Error

        NotEnoughCurrency = -3000,
        PurchaseLimitExceeded,
        ProductNotFound,

        #endregion

        #region Inventory Error

        NotEnoughItemCount = -4000,

        #endregion

        #region Unit Error

        MaxUnitStack = -5000,
        MaxLevelUnit,
        NotFoundUnit,
        NotEnoughUnitStack,

        #endregion

        #region Gacha Error

        GachaNotFound = -6000,
        GachaPoolEmpty,

        #endregion
        
        #region Event Error
        
        AlreadyCheckedToday = -7000,
        AlreadyClaimed,
        AttendanceEventNotFound,
        AttendanceEventEnded,
        NotCheckedYet,
        AttendanceRewardNotFound,
        
        #endregion
        
        #region Stage Error
        
        StageNotFound = -8000,
        StageLocked,
        
        #endregion
        
        #region Mail Error
        
        MailNotFound = -9000,
        MailAlreadyClaimed,
        MailRewardNotClaimed,
        
        #endregion
    }    
}