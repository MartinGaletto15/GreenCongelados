namespace Domain.Exceptions;

public class AppValidationException : Exception
{
    public string ErrorCode { get; set; } = string.Empty;
    public AppValidationException()
    : base()
    {
    }

    public AppValidationException(string message, string errorCode = "")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public AppValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}