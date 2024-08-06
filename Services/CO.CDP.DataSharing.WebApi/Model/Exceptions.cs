namespace CO.CDP.DataSharing.WebApi.Model;

public class SharedConsentNotFoundException(string message, Exception? cause = null) : Exception(message, cause);
