using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;

namespace CO.CDP.EntityVerification.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(InvalidIdentifierFormatException), (StatusCodes.Status400BadRequest, "INVALID_IDENTIFIER_FORMAT") },
    };

    public static IServiceCollection AddEntityVerificationProblemDetails(this IServiceCollection services)
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