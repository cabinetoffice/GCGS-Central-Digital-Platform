namespace CO.CDP.OrganisationApp.Logging;

public class ExceptionLogging : Exception
{
    public int ErrorCode { get; }

    public ExceptionLogging(string message, int errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public ExceptionLogging(string message, int errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}