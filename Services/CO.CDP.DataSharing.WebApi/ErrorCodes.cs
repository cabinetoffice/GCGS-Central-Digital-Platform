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
}