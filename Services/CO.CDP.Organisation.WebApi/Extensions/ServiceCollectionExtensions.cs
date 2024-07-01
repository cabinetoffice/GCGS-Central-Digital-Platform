using CO.CDP.Organisation.WebApi.Model;
using static CO.CDP.Organisation.WebApi.UseCase.RegisterOrganisationUseCase.RegisterOrganisationException;
using static CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository.OrganisationRepositoryException;

namespace CO.CDP.Organisation.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
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
        switch (exception)
        {
            case BadHttpRequestException:
                return (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY");
            case DuplicateOrganisationException:
                return (StatusCodes.Status400BadRequest, "ORGANISATION_ALREADY_EXISTS");
            case UnknownPersonException:
                return (StatusCodes.Status404NotFound, "PERSON_DOES_NOT_EXIST");
            case ArgumentNullException:
                return (StatusCodes.Status400BadRequest, "ARGUMENT_NULL");
            case InvalidOperationException:
                return (StatusCodes.Status400BadRequest, "INVALID_OPERATION");
            case UnknownOrganisationException:
                return (StatusCodes.Status404NotFound, "UNKNOWN_ORGANISATION");
            case BuyerInfoNotExistException:
                return (StatusCodes.Status404NotFound, "BUYER_INFO_NOT_EXISTS");
            case InvalidUpdateBuyerInformationCommand:
                return (StatusCodes.Status400BadRequest, "INVALID_BUYER_INFORMATION_UPDATE_ENTITY");
            case InvalidUpdateSupplierInformationCommand:
                return (StatusCodes.Status400BadRequest, "INVALID_SUPPLIER_INFORMATION_UPDATE_ENTITY");
            case InvalidQueryException:
                return (StatusCodes.Status400BadRequest, "MISSING_QUERY_PARAMETERS");
            default:
                return (StatusCodes.Status500InternalServerError, "GENERIC_ERROR");
        }
    }
}