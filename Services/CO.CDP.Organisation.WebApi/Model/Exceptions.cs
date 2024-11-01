namespace CO.CDP.Organisation.WebApi.Model;

public class UnknownOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
public class UnknownPersonException(string message, Exception? cause = null) : Exception(message, cause);
public class UnknownInvitedPersonException(string message, Exception? cause = null) : Exception(message, cause);
public class EmptyPersonRoleException(string message, Exception? cause = null) : Exception(message, cause);

public abstract class InvalidUpdateOrganisationCommand(string message, Exception? cause = null)
    : Exception(message, cause)
{
    public class UnknownOrganisationUpdateType()
        : InvalidUpdateOrganisationCommand("Unknown organisation update type.");

    public class MissingOrganisationName() : InvalidUpdateOrganisationCommand("Missing organisation name.");

    public class MissingRoles() : InvalidUpdateOrganisationCommand("Missing roles.");

    public class MissingContactPoint() : InvalidUpdateOrganisationCommand("Missing contact point.");

    public class NoPrimaryIdentifier()
        : InvalidUpdateOrganisationCommand("There are no identifiers remaining that can be set as the primary.");

    public class MissingOrganisationEmail() : InvalidUpdateOrganisationCommand("Missing organisation email.");

    public class OrganisationEmailDoesNotExist()
        : InvalidUpdateOrganisationCommand("Organisation email does not exists.");

    public class MissingOrganisationAddress() : InvalidUpdateOrganisationCommand("Missing organisation address.");

    public class MissingOrganisationRegisteredAddress()
        : InvalidUpdateOrganisationCommand("Missing Organisation registered address.");

    public class MissingAdditionalIdentifiers()
        : InvalidUpdateOrganisationCommand("Missing additional identifiers.");

    public class MissingIdentifierNumber()
        : InvalidUpdateOrganisationCommand("Missing identifier number.");

    public class IdentiferNumberAlreadyExists()
        : InvalidUpdateOrganisationCommand("The identifier you have entered belongs to a different organization that already exists.");
}
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
public class DuplicateInviteEmailForOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
public class PersonAlreadyAddedToOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
public class PersonAlreadyInvitedToOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
public class UnknownOrganisationJoinRequestException(string message, Exception? cause = null) : Exception(message, cause);