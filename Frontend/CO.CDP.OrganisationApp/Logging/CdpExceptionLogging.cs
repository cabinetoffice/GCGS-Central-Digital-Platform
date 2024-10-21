namespace CO.CDP.OrganisationApp.Logging;

public class CdpExceptionLogging : Exception
{
    public string ErrorCode { get; }

    public CdpExceptionLogging(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public CdpExceptionLogging(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public override string ToString()
    {
        return $"{ErrorCode}: Inner exception: {base.ToString()}";
    }
}