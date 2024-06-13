using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateOrganisationUseCase(IOrganisationRepository organisationRepository)
            : IUseCase<(Guid organisationId, UpdateOrganisation updateOrganisation), bool>
{
    public async Task<bool> Execute((Guid organisationId, UpdateOrganisation updateOrganisation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var updateObject = command.updateOrganisation.Organisation;

        switch (command.updateOrganisation.Type)
        {
            case OrganisationUpdateType.AdditionalIdentifiers:
                if (updateObject.AdditionalIdentifiers == null)
                {
                    throw new InvalidUpdateOrganisationCommand("Missing additional identifiers.");
                }
                foreach (var identifier in updateObject.AdditionalIdentifiers)
                {
                    var existing = organisation.Identifiers.FirstOrDefault(i => i.Scheme == identifier.Scheme);
                    if (existing != null)
                    {
                        existing.IdentifierId = identifier.Id;
                        existing.LegalName = identifier.LegalName;
                    }
                    else
                    {
                        organisation.Identifiers.Add(new OrganisationInformation.Persistence.Organisation.Identifier
                        {
                            IdentifierId = identifier.Id,
                            Primary = false,
                            LegalName = identifier.LegalName,
                            Scheme = identifier.Scheme
                        });
                    }
                }
                break;
            default:
                throw new InvalidUpdateOrganisationCommand("Unknown organisation update type.");
        }

        organisation.UpdateSupplierInformation();
        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}