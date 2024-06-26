using static CO.CDP.OrganisationInformation.Persistence.IPersonRepository.PersonRepositoryException;

namespace CO.CDP.Person.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
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
        switch (exception)
        {
            case BadHttpRequestException:
                return (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY");
            case DuplicatePersonException:
                return (StatusCodes.Status400BadRequest, "PERSON_ALREADY_EXISTS");
            default:
                return (StatusCodes.Status500InternalServerError, "GENERIC_ERROR");
        }
    }
}