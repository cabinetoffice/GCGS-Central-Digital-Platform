using CO.CDP.Person.WebApi.Model;
using static CO.CDP.OrganisationInformation.Persistence.IPersonRepository.PersonRepositoryException;

namespace CO.CDP.Person.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(DuplicatePersonException), (StatusCodes.Status400BadRequest, "PERSON_ALREADY_EXISTS") },
        { typeof(UnknownPersonException), (StatusCodes.Status404NotFound, "UNKNOWN_PERSON") },
        { typeof(UnknownPersonInviteException), (StatusCodes.Status404NotFound, "UNKNOWN_PERSON_INVITE") },
        { typeof(PersonInviteAlreadyClaimedException), (StatusCodes.Status400BadRequest, "PERSON_INVITE_ALREADY_CLAIMED") },
    };

    public static IServiceCollection AddPersonProblemDetails(this IServiceCollection services)
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