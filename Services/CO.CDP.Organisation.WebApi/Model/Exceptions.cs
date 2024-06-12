namespace CO.CDP.Organisation.WebApi.Model;

public class UnknownOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
public class BuyerInfoNotExistException(string message, Exception? cause = null) : Exception(message, cause);
public class InvalidUpdateBuyerInformationCommand(string message, Exception? cause = null) : Exception(message, cause);
public class SupplierInfoNotExistException(string message, Exception? cause = null) : Exception(message, cause);
public class InvalidUpdateSupplierInformationCommand(string message, Exception? cause = null) : Exception(message, cause);