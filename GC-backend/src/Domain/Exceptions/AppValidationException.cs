namespace Domain.Exceptions;

public class AppValidationException : Exception
{
    public string ErrorCode { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 400;

    public AppValidationException()
    : base()
    {
    }

    public AppValidationException(string message, string errorCode = "", int statusCode = 400)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public AppValidationException(string message, Exception innerException, string errorCode = "", int statusCode = 400)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
