using CO.CDP.Forms.WebApi.Model;
using static CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository.OrganisationRepositoryException;

namespace CO.CDP.Forms.WebApi;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(UnknownOrganisationException), (StatusCodes.Status404NotFound, "UNKNOWN_ORGANISATION") },
        { typeof(UnknownSectionException), (StatusCodes.Status404NotFound, "UNKNOWN_SECTION") },
        { typeof(UnknownQuestionsException), (StatusCodes.Status400BadRequest, "UNKNOWN_QUESTIONS") },
        { typeof(DuplicateOrganisationException), (StatusCodes.Status400BadRequest, "ORGANISATION_ALREADY_EXISTS") }
    };

    public static Dictionary<string, List<string>> HttpStatusCodeErrorMap() =>
        ExceptionMap.Values
            .GroupBy(s => s.Item1)
            .ToDictionary(k => k.Key.ToString(), v => v.Select(i => i.Item2).Distinct().ToList());
}