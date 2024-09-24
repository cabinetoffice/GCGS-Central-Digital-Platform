using CO.CDP.DataSharing.WebApi.Model;

namespace CO.CDP.DataSharing.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(UserUnauthorizedException), (StatusCodes.Status403Forbidden, "UNAUTHORIZED") },
        { typeof(InvalidOrganisationRequestedException), (StatusCodes.Status403Forbidden, "INVALID_ORGANISATION_REQUESTED") },
        { typeof(ShareCodeNotFoundException), (StatusCodes.Status404NotFound, Constants.ShareCodeNotFoundExceptionCode) }
    };

    public static IServiceCollection AddDataSharingProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                if (ctx.Exception != null)
                {
                    var (statusCode, errorCode) = MapException(ctx.Exception);
                    ctx.ProblemDetails.Status = statusCode;
                    ctx.HttpContext.Response.StatusCode = statusCode;
                    ctx.ProblemDetails.Extensions.Add("code", errorCode);
                }
            };
        });

        return services;
    }

    public static (int status, string error) MapException(Exception? exception)
    {
        if (ExceptionMap.TryGetValue(exception?.GetType() ?? typeof(Exception), out (int, string) code))
        {
            return code;
        }
        return (StatusCodes.Status500InternalServerError, "GENERIC_ERROR");
    }

    public static Dictionary<string, List<string>> ErrorCodes() =>
        ExceptionMap.Values
            .GroupBy(s => s.Item1)
            .ToDictionary(k => k.Key.ToString(), v => v.Select(i => i.Item2).Distinct().ToList());
}