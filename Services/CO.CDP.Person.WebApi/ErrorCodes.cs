using CO.CDP.Person.WebApi.Model;
using static CO.CDP.OrganisationInformation.Persistence.IPersonRepository.PersonRepositoryException;

namespace CO.CDP.Person.WebApi;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> Exception4xxMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(DuplicatePersonException), (StatusCodes.Status400BadRequest, "PERSON_ALREADY_EXISTS") },
        { typeof(UnknownPersonException), (StatusCodes.Status404NotFound, "UNKNOWN_PERSON") },
        { typeof(UnknownPersonInviteException), (StatusCodes.Status404NotFound, "UNKNOWN_PERSON_INVITE") },
        { typeof(PersonInviteAlreadyClaimedException), (StatusCodes.Status400BadRequest, "PERSON_INVITE_ALREADY_CLAIMED") },
        { typeof(DuplicateEmailWithinOrganisationException), (StatusCodes.Status400BadRequest, "PERSON_ALREADY_ADDED_TO_ORGANISATION") },
        { typeof(PersonInviteExpiredException), (StatusCodes.Status400BadRequest, "PERSON_INVITE_EXPIRED") },
    };
}