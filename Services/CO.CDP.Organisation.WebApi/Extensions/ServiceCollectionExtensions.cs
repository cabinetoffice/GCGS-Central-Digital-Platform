using CO.CDP.Organisation.WebApi.Model;
using static CO.CDP.Organisation.WebApi.UseCase.RegisterOrganisationUseCase.RegisterOrganisationException;
using static CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository.OrganisationRepositoryException;

namespace CO.CDP.Organisation.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(MissingOrganisationIdException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(DuplicateOrganisationException), (StatusCodes.Status400BadRequest, "ORGANISATION_ALREADY_EXISTS") },
        { typeof(UnknownPersonException), (StatusCodes.Status404NotFound, "PERSON_DOES_NOT_EXIST") },
        { typeof(SupplierInfoNotExistException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(UnknownOrganisationException), (StatusCodes.Status404NotFound, "UNKNOWN_ORGANISATION") },
        { typeof(BuyerInfoNotExistException), (StatusCodes.Status404NotFound, "BUYER_INFO_NOT_EXISTS") },
        { typeof(InvalidUpdateBuyerInformationCommand), (StatusCodes.Status400BadRequest, "INVALID_BUYER_INFORMATION_UPDATE_ENTITY") },
        { typeof(InvalidUpdateSupplierInformationCommand), (StatusCodes.Status400BadRequest, "INVALID_SUPPLIER_INFORMATION_UPDATE_ENTITY") },
        { typeof(InvalidQueryException), (StatusCodes.Status400BadRequest, "ISSUE_WITH_QUERY_PARAMETERS") },
    };

    public static IServiceCollection AddOrganisationProblemDetails(this IServiceCollection services)
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