using AutoMapper;
using CO.CDP.Authentication;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Model;

namespace CO.CDP.Tenant.WebApi.UseCase;

public class LookupTenantUseCase(ITenantRepository tenantRepository, IMapper mapper, IClaimService claimService)
    : IUseCase<Model.TenantLookup?>
{
    public async Task<Model.TenantLookup?> Execute()
    {
        var userUrn = claimService.GetUserUrn();

        if (string.IsNullOrEmpty(userUrn))
        {
            throw new UnknownTokenException("Cannot find sub or urn from JWT token.");
        }

        return await tenantRepository.LookupTenant(userUrn)
            .AndThen(mapper.Map<Model.TenantLookup>);
    }
}