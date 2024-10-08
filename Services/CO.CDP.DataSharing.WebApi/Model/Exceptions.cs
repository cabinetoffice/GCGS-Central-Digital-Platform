namespace CO.CDP.DataSharing.WebApi.Model;

public class UserUnauthorizedException : Exception;
public class InvalidOrganisationRequestedException(string message, Exception? cause = null) : Exception(message, cause);
public class ShareCodeNotFoundException(string message, Exception? cause = null) : Exception(message, cause);
public class SupplierInformationNotFoundException(string message, Exception? cause = null) : Exception(message, cause);