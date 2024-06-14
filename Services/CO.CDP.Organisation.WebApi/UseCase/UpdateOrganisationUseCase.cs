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
            case OrganisationUpdateType.Address:
                if (updateObject.Addresses == null)
                {
                    throw new InvalidUpdateOrganisationCommand("Missing organisation address.");
                }
                foreach (var address in updateObject.Addresses)
                {
                    var existing = organisation.Addresses.FirstOrDefault(i => i.Type == address.Type);
                    if (existing != null)
                    {
                        existing.Address.StreetAddress = address.StreetAddress;
                        existing.Address.StreetAddress2 = address.StreetAddress2;
                        existing.Address.PostalCode = address.PostalCode;
                        existing.Address.Locality = address.Locality;
                        existing.Address.Region = address.Region;
                        existing.Address.CountryName = address.CountryName;
                    }
                    else
                    {
                        organisation.Addresses.Add(new OrganisationInformation.Persistence.Organisation.OrganisationAddress
                        {
                            Type = address.Type,
                            Address = new Address
                            {
                                StreetAddress = address.StreetAddress,
                                StreetAddress2 = address.StreetAddress2,
                                PostalCode = address.PostalCode,
                                Locality = address.Locality,
                                Region = address.Region,
                                CountryName = address.CountryName
                            },
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