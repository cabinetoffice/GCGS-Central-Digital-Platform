using AutoMapper;
using CO.CDP.Authentication;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<Guid, Model.Organisation?>
{
    public async Task<Model.Organisation?> Execute(Guid organisationId)
    {
        return await organisationRepository.Find(organisationId)
            .AndThen(mapper.Map<Model.Organisation>);
    }
}

public class GetMyOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper, IClaimService claimService)
    : IUseCase<Model.Organisation?>
{
    public async Task<Model.Organisation?> Execute()
    {
        var organisationIdString = claimService.GetOrganisationId();

        if (!int.TryParse(organisationIdString, out var organisationId))
        {
            throw new MissingOrganisationIdException("Ensure the token is valid and contains the necessary claims.");
        }

        return await organisationRepository.Find(organisationId)
            .AndThen(mapper.Map<Model.Organisation>);
    }
}