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

        #endregion

        #region Player Error

        PlayerNotFound = -2001,
        NotEnoughStamina,

        #endregion

        #region Shop Error

        NotEnoughCurrency = -3001,
        PurchaseLimitExceeded,
        ProductNotFound,

        #endregion

        #region Inventory Error

        NotEnoughItemCount = -4001,

        #endregion

        #region Unit Error

        MaxUnitStack = -5001,
        MaxLevelUnit,
        NotFoundUnit,
        NotEnoughUnitStack,

        #endregion

        #region Gacha Error

        GachaNotFound = -6001,
        GachaPoolEmpty,

        #endregion

        #region Stage Error

        StageNotFound = -7001,
        StageLocked,

        #endregion

        #region Event Error

        AttendanceEventNotFound = -8001,
        AttendanceEventEnded,
        AlreadyCheckedToday,
        NotCheckedYet,
        AlreadyClaimed,
        AttendanceRewardNotFound,

        #endregion

        #region Mail Error

        MailNotFound = -9001,
        MailExpired,
        MailAlreadyClaimed,
        MailRewardNotClaimed,

        #endregion

        #region Auth Error

        AuthTokenInvalid = -10001,

        #endregion
    }    
}