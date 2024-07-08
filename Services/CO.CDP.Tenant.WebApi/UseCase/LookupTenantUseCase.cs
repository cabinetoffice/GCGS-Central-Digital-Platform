using AutoMapper;
using CO.CDP.Authentication;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Model;

namespace CO.CDP.Tenant.WebApi.UseCase;

public class LookupTenantUseCase(ITenantRepository tenantRepository, IMapper mapper, IClaimService claimService)
    : IUseCase<OrganisationInformation.TenantLookup?>
{
    public async Task<OrganisationInformation.TenantLookup?> Execute()
    {
        var userUrn = claimService.GetUserUrn();

        if (string.IsNullOrEmpty(userUrn))
        {
            throw new MissingUserUrnException("Ensure the token is valid and contains the necessary claims.");
        }

        return await tenantRepository.LookupTenant(userUrn)
            .AndThen(mapper.Map<OrganisationInformation.TenantLookup>);
    }
}