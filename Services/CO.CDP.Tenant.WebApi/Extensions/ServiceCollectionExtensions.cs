using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CO.CDP.OrganisationInformation.Persistence.ITenantRepository.TenantRepositoryException;

namespace CO.CDP.Tenant.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
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
        switch (exception)
        {
            case BadHttpRequestException:
                return (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY");
            case DuplicateTenantException:
                return (StatusCodes.Status404NotFound, "TENANT_ALREADY_EXISTS");
            case ArgumentNullException:
                return (StatusCodes.Status400BadRequest, "ARGUMENT_NULL");
            case InvalidOperationException:
                return (StatusCodes.Status400BadRequest, "INVALID_OPERATION");
            default:
                return (StatusCodes.Status500InternalServerError, "GENERIC_ERROR");
        }
    }
}