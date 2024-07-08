using CO.CDP.Tenant.WebApi.Model;
using static CO.CDP.OrganisationInformation.Persistence.ITenantRepository.TenantRepositoryException;

namespace CO.CDP.Tenant.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(DuplicateTenantException), (StatusCodes.Status400BadRequest, "TENANT_ALREADY_EXISTS") },
        { typeof(ArgumentNullException), (StatusCodes.Status400BadRequest, "ARGUMENT_NULL") },
        { typeof(InvalidOperationException), (StatusCodes.Status400BadRequest, "INVALID_OPERATION") },
        { typeof(TenantNotFoundException), (StatusCodes.Status404NotFound, "TENANT_DOES_NOT_EXIST") },
        { typeof(MissingUserUrnException), (StatusCodes.Status401Unauthorized, "NOT_AUTHENTICATED") },

    };

    public static IServiceCollection AddTenantProblemDetails(this IServiceCollection services)
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