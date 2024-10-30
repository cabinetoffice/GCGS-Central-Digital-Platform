using CO.CDP.Organisation.WebApi.Model;
using static CO.CDP.Organisation.WebApi.Model.InvalidUpdateOrganisationCommand;
using static CO.CDP.OrganisationInformation.Persistence.IAuthenticationKeyRepository.AuthenticationKeyRepositoryException;
using static CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository.OrganisationRepositoryException;

namespace CO.CDP.Organisation.WebApi;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> Exception4xxMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(MissingOrganisationIdException), (StatusCodes.Status404NotFound, "ORGANISATION_NOT_FOUND") },
        { typeof(DuplicateOrganisationException), (StatusCodes.Status400BadRequest, "ORGANISATION_ALREADY_EXISTS") },
        { typeof(UnknownPersonException), (StatusCodes.Status404NotFound, "PERSON_DOES_NOT_EXIST") },
        { typeof(SupplierInfoNotExistException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(UnknownOrganisationException), (StatusCodes.Status404NotFound, "UNKNOWN_ORGANISATION") },
        { typeof(UnknownInvitedPersonException), (StatusCodes.Status404NotFound, "UNKNOWN_INVITED_PERSON") },
        { typeof(EmptyPersonRoleException), (StatusCodes.Status404NotFound, "EMPTY_INVITED_PERSON_ROLE") },
        { typeof(BuyerInfoNotExistException), (StatusCodes.Status404NotFound, "BUYER_INFO_NOT_EXISTS") },
        { typeof(InvalidUpdateBuyerInformationCommand), (StatusCodes.Status400BadRequest, "INVALID_BUYER_INFORMATION_UPDATE_ENTITY") },
        { typeof(InvalidUpdateSupplierInformationCommand), (StatusCodes.Status400BadRequest, "INVALID_SUPPLIER_INFORMATION_UPDATE_ENTITY") },
        { typeof(InvalidQueryException), (StatusCodes.Status400BadRequest, "ISSUE_WITH_QUERY_PARAMETERS") },
        { typeof(DuplicateAuthenticationKeyNameException), (StatusCodes.Status400BadRequest, "APIKEY_NAME_ALREADY_EXISTS") },
        { typeof(DuplicateInviteEmailForOrganisationException), (StatusCodes.Status400BadRequest, "INVITE_EMAIL_ALREADY_EXISTS_FOR_ORGANISATION") },
        { typeof(PersonAlreadyAddedToOrganisationException), (StatusCodes.Status400BadRequest, "PERSON_ALREADY_ADDED_TO_ORGANISATION") },
        { typeof(DuplicateEmailWithinOrganisationException), (StatusCodes.Status400BadRequest, "EMAIL_ALREADY_EXISTS_WITHIN_ORGANISATION") },
        { typeof(InvalidUpdateOrganisationCommand), (StatusCodes.Status400BadRequest, "ORGANISATION_UPDATE_INVALID_INPUT") },
        { typeof(MissingOrganisationName), (StatusCodes.Status400BadRequest, "ORGANISATION_MISSING_NAME") },
        { typeof(MissingRoles), (StatusCodes.Status400BadRequest, "ORGANISATION_MISSING_ROLES") },
        { typeof(MissingContactPoint), (StatusCodes.Status400BadRequest, "ORGANISATION_MISSING_CONTACTPOINT") },
        { typeof(NoPrimaryIdentifier), (StatusCodes.Status400BadRequest, "ORGANISATION_NOPRIMARY_IDENTIFIER") },
        { typeof(MissingOrganisationEmail), (StatusCodes.Status400BadRequest, "ORGANISATION_MISSING_EMAIL") },
        { typeof(OrganisationEmailDoesNotExist), (StatusCodes.Status400BadRequest, "ORGANISATION_EMAIL_DOES_NOT_EXISTS") },
        { typeof(MissingOrganisationAddress), (StatusCodes.Status400BadRequest, "ORGANISATION_MISSING_ADDRESS") },
        { typeof(MissingOrganisationRegisteredAddress), (StatusCodes.Status400BadRequest, "ORGANISATION_MISSING_REGISTERED_ADDRESS") },
        { typeof(MissingAdditionalIdentifiers), (StatusCodes.Status400BadRequest, "ORGANISATION_MISSING_ADDITIONAL_IDENTIFIERS") },
        { typeof(MissingIdentifierNumber), (StatusCodes.Status400BadRequest, "ORGANISATION_MISSING_IDENTIFIER_NUMBER") },
        { typeof(IdentiferNumberAlreadyExists), (StatusCodes.Status400BadRequest, "ORGANISATION_IDENTIFIER_NUMBER_ALREADY_EXISTS") }
    };
}