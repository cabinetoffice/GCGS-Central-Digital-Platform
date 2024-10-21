using AutoMapper;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
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
            case OrganisationUpdateType.OrganisationName:
                if (string.IsNullOrEmpty(updateObject.OrganisationName))
                {
                    throw new InvalidUpdateOrganisationCommand("Missing organisation name.");
                }
                organisation.Name = updateObject.OrganisationName;
                break;

            case OrganisationUpdateType.Roles:
                if (updateObject.Roles == null || !updateObject.Roles.Any())
                {
                    throw new InvalidUpdateOrganisationCommand("Missing roles.");
                }

                if (!updateObject.Roles.All(role => Enum.IsDefined(typeof(PartyRole), role)))
                {
                    throw new InvalidUpdateOrganisationCommand("One or more roles are invalid.");
                }

                organisation.Roles = updateObject.Roles;
                break;

            case OrganisationUpdateType.OrganisationEmail:
                if (updateObject.ContactPoint == null || string.IsNullOrEmpty(updateObject.ContactPoint.Email))
                    throw new InvalidUpdateOrganisationCommand("Missing organisation email.");

                var organisationContact = organisation.ContactPoints.FirstOrDefault();
                if (organisationContact == null)
                    throw new InvalidUpdateOrganisationCommand("organisation email does not exists.");

                organisationContact.Email = updateObject.ContactPoint.Email;
                break;

            case OrganisationUpdateType.RegisteredAddress:
                if (updateObject.Addresses == null)
                    throw new InvalidUpdateOrganisationCommand("Missing organisation address.");

                var newAddress = updateObject.Addresses.FirstOrDefault(x => x.Type == AddressType.Registered);
                if (newAddress == null)
                    throw new InvalidUpdateOrganisationCommand("Missing Organisation registered address.");

                var existingAddress = organisation.Addresses.FirstOrDefault(i => i.Type == newAddress.Type);
                if (existingAddress != null)
                {
                    existingAddress.Address.StreetAddress = newAddress.StreetAddress;
                    existingAddress.Address.PostalCode = newAddress.PostalCode;
                    existingAddress.Address.Locality = newAddress.Locality;
                    existingAddress.Address.Region = newAddress.Region;
                    existingAddress.Address.CountryName = newAddress.CountryName;
                    existingAddress.Address.Country = newAddress.Country;
                }
                else
                {
                    organisation.Addresses.Add(new OrganisationInformation.Persistence.Organisation.OrganisationAddress
                    {
                        Type = newAddress.Type,
                        Address = new Address
                        {
                            StreetAddress = newAddress.StreetAddress,
                            PostalCode = newAddress.PostalCode,
                            Locality = newAddress.Locality,
                            Region = newAddress.Region,
                            CountryName = newAddress.CountryName,
                            Country = newAddress.Country
                        },
                    });
                }
                break;
            case OrganisationUpdateType.RemoveIdentifier:
                var identifierToRemove = organisation.Identifiers.FirstOrDefault(
                    i => i.Scheme == command.updateOrganisation.Organisation.IdentifierToRemove!.Scheme);

                if (identifierToRemove != null)
                {
                    RemoveIdentifier(organisation, identifierToRemove);
                }

                break;
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
                    }
                    else if (!string.IsNullOrEmpty(identifier.Id))
                    {
                        organisation.Identifiers.Add(new OrganisationInformation.Persistence.Organisation.Identifier
                        {
                            IdentifierId = identifier.Id,
                            Primary = AssignIdentifierUseCase.IsPrimaryIdentifier(organisation, identifier.Scheme),
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

        await organisationRepository.SaveAsync(organisation,
            async o => await publisher.Publish(mapper.Map<OrganisationUpdated>(o)));

        return await Task.FromResult(true);
    }

    private void RemoveIdentifier(OrganisationInformation.Persistence.Organisation organisation,
        OrganisationInformation.Persistence.Organisation.Identifier identifierToRemove)
    {
        organisation.Identifiers.Remove(identifierToRemove);

        if (identifierToRemove.Primary)
        {
            AllocateNextPrimaryIdentifier(organisation);
        }
    }

    private void AllocateNextPrimaryIdentifier(OrganisationInformation.Persistence.Organisation organisation)
    {
        var nextPrimaryIdentifier = organisation.Identifiers.FirstOrDefault(i =>
            i.Scheme != AssignIdentifierUseCase.IdentifierSchemes.Ppon &&
            i.Scheme != AssignIdentifierUseCase.IdentifierSchemes.Other &&
            i.Scheme != AssignIdentifierUseCase.IdentifierSchemes.Vat);

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
            throw new InvalidUpdateOrganisationCommand("There are no identifiers remaining that can be set as the primary.");
        }
    }
}