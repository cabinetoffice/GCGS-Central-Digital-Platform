using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationsUseCase(IOrganisationRepository organisationRepository)
    : IUseCase<PaginatedOrganisationQuery, IEnumerable<OrganisationDto>>
{
    public async Task<IEnumerable<OrganisationDto>> Execute(PaginatedOrganisationQuery command)
    {
        var organisations = await organisationRepository
            .GetPaginatedRaw(command.Role, command.PendingRole, command.SearchText, command.Limit, command.Skip);

        return organisations.Select(o => new OrganisationDto()
        {
            Id = o.Id,
            Guid = o.Guid,
            Name = o.Name,
            Roles = o.Roles?.Select(r => (PartyRole)r).ToList() ?? new List<PartyRole>(),
            PendingRoles = o.PendingRoles?.Select(r => (PartyRole)r).ToList() ?? new List<PartyRole>(),
            ApprovedOn = o.ApprovedOn,
            ReviewComment = o.ReviewComment,
            ReviewedByFirstName = o.ReviewedByFirstName,
            ReviewedByLastName = o.ReviewedByLastName,
            Identifiers = o.Identifiers?.Split(", ").ToList() ?? new List<string>(),
            ContactPoints = o.ContactPoints?.Split(", ").ToList() ?? new List<string>()
        }).ToList();
    }

}