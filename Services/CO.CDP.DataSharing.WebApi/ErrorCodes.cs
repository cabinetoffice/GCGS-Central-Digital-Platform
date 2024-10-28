using CO.CDP.DataSharing.WebApi.Model;

namespace CO.CDP.DataSharing.WebApi;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(UserUnauthorizedException), (StatusCodes.Status403Forbidden, "UNAUTHORIZED") },
        { typeof(InvalidOrganisationRequestedException), (StatusCodes.Status403Forbidden, "INVALID_ORGANISATION_REQUESTED") },
        { typeof(ShareCodeNotFoundException), (StatusCodes.Status404NotFound, Constants.ShareCodeNotFoundExceptionCode) }
    };

    public static Dictionary<string, List<string>> HttpStatusCodeErrorMap() =>
        ExceptionMap.Values
            .GroupBy(s => s.Item1)
            .ToDictionary(k => k.Key.ToString(), v => v.Select(i => i.Item2).Distinct().ToList());
}