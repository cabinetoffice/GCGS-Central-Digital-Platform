namespace CO.CDP.Tenant.WebApi.Model;

public class MissingUserUrnException(string message, Exception? cause = null) : Exception(message, cause);