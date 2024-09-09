using AutoMapper;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPublisher publisher,
    IMapper mapper
) : IUseCase<(Guid organisationId, UpdateOrganisation updateOrganisation), bool>
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
                    var existingIdentifier = organisation.Identifiers.FirstOrDefault(i => i.Scheme == identifier.Scheme);
                    if (existingIdentifier != null)
                    {
                        if (!string.IsNullOrEmpty(identifier.Id))
                        {
                            existingIdentifier.IdentifierId = identifier.Id;
                            existingIdentifier.LegalName = identifier.LegalName;
                        }
                        else
                        {
                            RemoveIdentifier(organisation, existingIdentifier);
                        }
                    }
                    else if (!string.IsNullOrEmpty(identifier.Id))
                    {
                        organisation.Identifiers.Add(new OrganisationInformation.Persistence.Organisation.Identifier
                        {
                            IdentifierId = identifier.Id,
                            Primary = AssignIdentifierUseCase.IsPrimaryIdentifier(organisation),
                            LegalName = identifier.LegalName,
                            Scheme = identifier.Scheme
                        });
                    }
                }
                break;

            case OrganisationUpdateType.ContactPoint:
                if (updateObject.ContactPoint == null)
                {
                    throw new InvalidUpdateOrganisationCommand("Missing contact point.");
                }

                var existingContact = organisation.ContactPoints.FirstOrDefault();
                if (existingContact != null)
                {
                    existingContact.Name = updateObject.ContactPoint.Name;
                    existingContact.Email = updateObject.ContactPoint.Email;
                    existingContact.Telephone = updateObject.ContactPoint.Telephone;
                    existingContact.Url = updateObject.ContactPoint.Url;
                }
                else
                {
                    organisation.ContactPoints.Add(
                        mapper.Map<OrganisationInformation.Persistence.Organisation.ContactPoint>(updateObject.ContactPoint));
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
                        existing.Address.PostalCode = address.PostalCode;
                        existing.Address.Locality = address.Locality;
                        existing.Address.Region = address.Region;
                        existing.Address.CountryName = address.CountryName;
                        existing.Address.Country = address.Country;
                    }
                    else
                    {
                        organisation.Addresses.Add(new OrganisationInformation.Persistence.Organisation.OrganisationAddress
                        {
                            Type = address.Type,
                            Address = new Address
                            {
                                StreetAddress = address.StreetAddress,
                                PostalCode = address.PostalCode,
                                Locality = address.Locality,
                                Region = address.Region,
                                CountryName = address.CountryName,
                                Country = address.Country
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
        await publisher.Publish(mapper.Map<OrganisationUpdated>(organisation));

        return await Task.FromResult(true);
    }

    private void RemoveIdentifier(OrganisationInformation.Persistence.Organisation organisation,
        OrganisationInformation.Persistence.Organisation.Identifier identifierToRemove)
    {
        organisation.Identifiers.Remove(identifierToRemove);

        if (identifierToRemove.Primary)
        {
            var nextPrimaryIdentifier = organisation.Identifiers.FirstOrDefault(i =>
                i.Scheme != AssignIdentifierUseCase.IdentifierSchemes.Ppon &&
                i.Scheme != AssignIdentifierUseCase.IdentifierSchemes.Other);

            if (nextPrimaryIdentifier == null)
            {
                nextPrimaryIdentifier = organisation.Identifiers.FirstOrDefault(i =>
                    i.Scheme == AssignIdentifierUseCase.IdentifierSchemes.Ppon);

                if (nextPrimaryIdentifier == null)
                {
                    nextPrimaryIdentifier = organisation.Identifiers.FirstOrDefault(i =>
                        i.Scheme == AssignIdentifierUseCase.IdentifierSchemes.Other);
                }
            }

            if (nextPrimaryIdentifier != null)
            {
                nextPrimaryIdentifier.Primary = true;
            }
            else
            {
                throw new InvalidUpdateOrganisationCommand("Identifier cannot be removed as there is no identifier remaining to set as the primary.");
            }
        }
    }
}