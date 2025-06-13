using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationsUseCase(IOrganisationRepository organisationRepository)
    : IUseCase<PaginatedOrganisationQuery, Tuple<IEnumerable<OrganisationDto>, int>>
{
    public async Task<Tuple<IEnumerable<OrganisationDto>, int>> Execute(PaginatedOrganisationQuery command)
    {
        var organisations = await organisationRepository
            .GetPaginated(command.Role, command.PendingRole, command.SearchText, command.Limit, command.Skip);

        return new Tuple<IEnumerable<OrganisationDto>, int>(organisations.Item1.Select(o => new OrganisationDto()
        {
            Id = o.Guid,
            Name = o.Name,
            Type = o.Type,
            Roles = o.Roles?.Select(r => (PartyRole)r).ToList() ?? new List<PartyRole>(),
            PendingRoles = o.PendingRoles?.Select(r => (PartyRole)r).ToList() ?? new List<PartyRole>(),
            ApprovedOn = o.ApprovedOn,
            ReviewComment = o.ReviewComment,
            ReviewedByFirstName = o.ReviewedByFirstName,
            ReviewedByLastName = o.ReviewedByLastName,
            Identifiers = o.Identifiers?.Split(", ").ToList() ?? new List<string>(),
            ContactPoints = o.ContactPoints?.Split(", ").ToList() ?? new List<string>(),
            AdminEmail = o.AdminEmail
        }).ToList(), organisations.Item2);
    }

}