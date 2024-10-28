using CO.CDP.Tenant.WebApi.Model;
using static CO.CDP.OrganisationInformation.Persistence.ITenantRepository.TenantRepositoryException;

namespace CO.CDP.Tenant.WebApi;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
        { typeof(DuplicateTenantException), (StatusCodes.Status400BadRequest, "TENANT_ALREADY_EXISTS") },
        { typeof(TenantNotFoundException), (StatusCodes.Status404NotFound, "TENANT_DOES_NOT_EXIST") },
        { typeof(MissingUserUrnException), (StatusCodes.Status401Unauthorized, "NOT_AUTHENTICATED") },
    };
}