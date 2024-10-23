namespace CO.CDP.Organisation.WebApi.Model;

public class UnknownOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
public class UnknownPersonException(string message, Exception? cause = null) : Exception(message, cause);
public class UnknownInvitedPersonException(string message, Exception? cause = null) : Exception(message, cause);
public class EmptyPersonRoleException(string message, Exception? cause = null) : Exception(message, cause);
public class InvalidUpdateOrganisationCommand(string message, Exception? cause = null) : Exception(message, cause);
public class BuyerInfoNotExistException(string message, Exception? cause = null) : Exception(message, cause);
public class InvalidUpdateBuyerInformationCommand(string message, Exception? cause = null) : Exception(message, cause);
public class SupplierInfoNotExistException(string message, Exception? cause = null) : Exception(message, cause);
public class InvalidUpdateSupplierInformationCommand(string message, Exception? cause = null) : Exception(message, cause);
public class InvalidQueryException(string message, Exception? cause = null) : Exception(message, cause);
public class InvalidUpdateConnectedEntityCommand(string message, Exception? cause = null) : Exception(message, cause);

public class UnknownConnectedEntityException(string message, Exception? cause = null) : Exception(message, cause);

public class MissingOrganisationIdException(string message, Exception? cause = null) : Exception(message, cause);

public class EmptyAuthenticationKeyNameException(string message, Exception? cause = null) : Exception(message, cause);
public class UnknownAuthenticationKeyException(string message, Exception? cause = null) : Exception(message, cause);
public class InvalidSupportUpdateOrganisationCommand(string message, Exception? cause = null) : Exception(message, cause);
public class DuplicateEmailWithinOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
public class PersonAlreadyAddedToOrganisationException(string message, Exception? cause = null) : Exception(message, cause);