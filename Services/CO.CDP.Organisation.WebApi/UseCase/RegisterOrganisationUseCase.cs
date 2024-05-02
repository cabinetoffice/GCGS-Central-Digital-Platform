using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RegisterOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper, Func<Guid> guidFactory)
    : IUseCase<NewOrganisation, Model.Organisation>
{
    public RegisterOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
       : this(organisationRepository, mapper, Guid.NewGuid)
    {
    }

    public Task<Model.Organisation> Execute(NewOrganisation command)
    {
        var organisation = mapper.Map<OrganisationInformation.Persistence.Organisation>(command, o => o.Items["Guid"] = guidFactory());
        organisationRepository.Save(organisation);
        return Task.FromResult(mapper.Map<Model.Organisation>(organisation));
    }
}