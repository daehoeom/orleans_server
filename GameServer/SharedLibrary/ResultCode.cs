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
}