using AutoMapper;
using CO.CDP.Authentication;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class GetMyOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper, IClaimService claimService)
    : IUseCase<Model.Organisation?>
{
    public async Task<Model.Organisation?> Execute()
    {
        var organisationId = claimService.GetOrganisationId();

        if (organisationId == null)
        {
            throw new MissingOrganisationIdException("Ensure the token is valid and contains the necessary claims.");
        }

        return await organisationRepository.Find(organisationId.Value)
            .AndThen(mapper.Map<Model.Organisation>);
    }
}