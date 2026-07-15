namespace SharedLibrary;

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
}
