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
    
    #endregion
}