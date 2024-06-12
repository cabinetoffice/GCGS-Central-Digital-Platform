using static CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository.OrganisationRepositoryException;
using static CO.CDP.Organisation.WebApi.UseCase.RegisterOrganisationUseCase.RegisterOrganisationException;
using static CO.CDP.OrganisationInformation.Persistence.ITenantRepository.TenantRepositoryException;
using static CO.CDP.Organisation.WebApi.UseCase.UpdateBuyerInformationUseCase.UpdateBuyerInformationException;

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
            case UnknownBuyerInformationUpdateTypeException:
                return (StatusCodes.Status400BadRequest, "UNKNOWN_BUYER_INFORMATION_UPDATE_TYPE");
            default:
                return (StatusCodes.Status500InternalServerError, "GENERIC_ERROR");
        }
    }
}