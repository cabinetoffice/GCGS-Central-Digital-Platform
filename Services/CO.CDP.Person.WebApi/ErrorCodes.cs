using CO.CDP.Person.WebApi.Model;
using static CO.CDP.OrganisationInformation.Persistence.IPersonRepository.PersonRepositoryException;

namespace CO.CDP.Person.WebApi;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(DuplicatePersonException), (StatusCodes.Status400BadRequest, "PERSON_ALREADY_EXISTS") },
        { typeof(UnknownPersonException), (StatusCodes.Status404NotFound, "UNKNOWN_PERSON") },
        { typeof(UnknownPersonInviteException), (StatusCodes.Status404NotFound, "UNKNOWN_PERSON_INVITE") },
        { typeof(PersonInviteAlreadyClaimedException), (StatusCodes.Status400BadRequest, "PERSON_INVITE_ALREADY_CLAIMED") },
    };

    public static Dictionary<string, List<string>> HttpStatusCodeErrorMap() =>
        ExceptionMap.Values
            .GroupBy(s => s.Item1)
            .ToDictionary(k => k.Key.ToString(), v => v.Select(i => i.Item2).Distinct().ToList());
}