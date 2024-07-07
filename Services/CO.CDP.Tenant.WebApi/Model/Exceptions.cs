namespace CO.CDP.Tenant.WebApi.Model;

public class UnknownTokenException(string message, Exception? cause = null) : Exception(message, cause);