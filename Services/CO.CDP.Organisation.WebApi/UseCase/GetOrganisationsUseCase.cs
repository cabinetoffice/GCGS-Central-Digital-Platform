using AutoMapper;
using CO.CDP.Authentication;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Person = CO.CDP.Organisation.WebApi.Model.Person;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationsUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<PaginatedOrganisationQuery, IEnumerable<OrganisationExtended>>
{
    public async Task<IEnumerable<OrganisationExtended>> Execute(PaginatedOrganisationQuery command)
    {
        var organisations = await organisationRepository
            .GetPaginated(command.Role, command.PendingRole, command.Limit, command.Skip);

        return organisations.Select(org =>
        {
            var adminPerson = org.OrganisationPersons
                .Where(op => op.Scopes.Contains(Constants.OrganisationPersonScope.Admin))
                .Select(op => op.Person)
                .FirstOrDefault();

            var adminPersonWeb = mapper.Map<Person>(adminPerson);

            return mapper.Map<OrganisationExtended>(org) with { AdminPerson = adminPersonWeb };
        });
    }
}