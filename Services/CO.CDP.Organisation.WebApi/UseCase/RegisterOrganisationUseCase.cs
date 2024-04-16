using AutoMapper;
using CO.CDP.Organisation.Persistence;
using CO.CDP.Organisation.WebApi.Model;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RegisterOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper, Func<Guid> guidFactory)
    : IUseCase<RegisterOrganisation, Model.Organisation>
{
    public RegisterOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
       : this(organisationRepository, mapper, Guid.NewGuid)
    {
    }

    public Task<Model.Organisation> Execute(RegisterOrganisation command)
    {
        var organisation = mapper.Map<Persistence.Organisation>(command, o => o.Items["Guid"] = guidFactory());
        organisationRepository.Save(organisation);
        return Task.FromResult(mapper.Map<Model.Organisation>(organisation));
    }
}