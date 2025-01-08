using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class GetOrganisationPartiesUseCase(
    IOrganisationPartiesRepository orgPartiesRepo,
    IMapper mapper
    ) : IUseCase<Guid, OrganisationParties?>
{
    public async Task<OrganisationParties?> Execute(Guid command)
    {
        var organisationParties = await orgPartiesRepo.Find(command);
        if (!organisationParties.Any())
        {
            return null;
        }

        var parties = mapper.Map<IEnumerable<Model.OrganisationParty>>(organisationParties);

        return new OrganisationParties { Parties = parties };
    }
}