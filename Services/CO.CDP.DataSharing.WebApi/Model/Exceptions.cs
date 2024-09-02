namespace CO.CDP.DataSharing.WebApi.Model;

public class InvalidOrganisationRequestedException(string message, Exception? cause = null) : Exception(message, cause);
public class SharedConsentNotFoundException(string message, Exception? cause = null) : Exception(message, cause);
public class SupplierInformationNotFoundException(string message, Exception? cause = null) : Exception(message, cause);