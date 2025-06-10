using AutoMapper;
using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPublisher publisher,
    IMapper mapper,
    IConfiguration configuration,
    IGovUKNotifyApiClient govUKNotifyApiClient,
    ILogger<UpdateOrganisationUseCase> logger
) : IUseCase<(Guid organisationId, UpdateOrganisation updateOrganisation), bool>
{
    public async Task<bool> Execute((Guid organisationId, UpdateOrganisation updateOrganisation) command)
    {
        var organisation = await organisationRepository.FindIncludingTenant(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var updateObject = command.updateOrganisation.Organisation;

        switch (command.updateOrganisation.Type)
        {
            case OrganisationUpdateType.OrganisationName:
                if (string.IsNullOrEmpty(updateObject.OrganisationName))
                {
                    throw new InvalidUpdateOrganisationCommand.MissingOrganisationName();
                }
                organisation.Name = updateObject.OrganisationName;
                organisation.Tenant.Name = updateObject.OrganisationName;
                organisation.Identifiers.Select(x => x.LegalName = updateObject.OrganisationName).ToList();
                break;

            case OrganisationUpdateType.AddRoles:
                if (updateObject.Roles == null || !updateObject.Roles.Any())
                {
                    throw new InvalidUpdateOrganisationCommand.MissingRoles();
                }

                organisation.Roles.AddRange(updateObject.Roles);
                break;

            case OrganisationUpdateType.AddAsBuyerRole:
                if (updateObject.BuyerInformation?.BuyerType == null || updateObject.BuyerInformation.DevolvedRegulations == null)
                {
                    throw new InvalidUpdateOrganisationCommand.MissingBuyerInformation();
                }

                organisation.ApprovedOn = null;
                organisation.ReviewComment = null;
                organisation.ReviewedById = null;

                organisation.BuyerInfo = new Persistence.BuyerInformation
                {
                    BuyerType = updateObject.BuyerInformation.BuyerType,
                    DevolvedRegulations = updateObject.BuyerInformation.DevolvedRegulations,
                    Id = organisation.Id
                };

                organisation.PendingRoles.Add(PartyRole.Buyer);

                await SendBuyerApprovalEmail(organisation);

                break;

            case OrganisationUpdateType.OrganisationEmail:
                if (updateObject.ContactPoint == null || string.IsNullOrEmpty(updateObject.ContactPoint.Email))
                    throw new InvalidUpdateOrganisationCommand.MissingOrganisationEmail();

                var organisationContact = organisation.ContactPoints.FirstOrDefault();
                if (organisationContact == null)
                    throw new InvalidUpdateOrganisationCommand.OrganisationEmailDoesNotExist();

                organisationContact.Email = updateObject.ContactPoint.Email;
                break;

            case OrganisationUpdateType.RegisteredAddress:
                if (updateObject.Addresses == null)
                    throw new InvalidUpdateOrganisationCommand.MissingOrganisationAddress();

                var newAddress = updateObject.Addresses.FirstOrDefault(x => x.Type == AddressType.Registered);
                if (newAddress == null)
                    throw new InvalidUpdateOrganisationCommand.MissingOrganisationRegisteredAddress();

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
                    organisation.Addresses.Add(new OrganisationInformation.Persistence.OrganisationAddress
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
                if (updateObject.AdditionalIdentifiers == null || !updateObject.AdditionalIdentifiers.Any())
                {
                    throw new InvalidUpdateOrganisationCommand.MissingAdditionalIdentifiers();
                }

                foreach (var identifier in updateObject.AdditionalIdentifiers)
                {
                    // Exclude "VAT" identifiers, they can be shared between organisations
                    if (identifier.Scheme != "VAT")
                    {
                        await ValidateIdentifierIsNotKnownToUs(identifier);
                    }

                    var existingIdentifier = organisation.Identifiers.FirstOrDefault(i => i.Scheme == identifier.Scheme);
                    if (existingIdentifier != null)
                    {
                        existingIdentifier.IdentifierId = identifier.Id;
                        existingIdentifier.LegalName = identifier.LegalName;
                    }
                    else
                    {
                        organisation.Identifiers.Add(new OrganisationInformation.Persistence.Identifier
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
                    throw new InvalidUpdateOrganisationCommand.MissingContactPoint();
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
                        mapper.Map<OrganisationInformation.Persistence.ContactPoint>(updateObject.ContactPoint));
                }
                break;

            case OrganisationUpdateType.Address:
                if (updateObject.Addresses == null)
                {
                    throw new InvalidUpdateOrganisationCommand.MissingOrganisationAddress();
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
                        organisation.Addresses.Add(new OrganisationInformation.Persistence.OrganisationAddress
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
                throw new InvalidUpdateOrganisationCommand.UnknownOrganisationUpdateType();
        }

        organisation.UpdateSupplierInformation();

        await ResetRejectedStatus(command.updateOrganisation.Type, organisation);

        await organisationRepository.SaveAsync(organisation,
            async o => await publisher.Publish(mapper.Map<OrganisationUpdated>(o)));

        return await Task.FromResult(true);
    }

    private async Task ResetRejectedStatus(OrganisationUpdateType updateType, Persistence.Organisation organisation)
    {
        if (
            (updateType == OrganisationUpdateType.OrganisationName || updateType == OrganisationUpdateType.OrganisationEmail)
            && organisation.PendingRoles.Contains(PartyRole.Buyer)
        )
        {
            var review = mapper.Map<Review>(await organisationRepository.FindIncludingReviewedBy(organisation.Guid));

            if (review.Status == ReviewStatus.Rejected)
            {
                await SendBuyerApprovalEmail(organisation);

                organisation.ReviewComment = null;
                organisation.ReviewedBy = null;
            }
        }
    }

    private async Task SendBuyerApprovalEmail(Persistence.Organisation organisation)
    {
        var baseAppUrl = configuration.GetValue<string>("OrganisationAppUrl") ?? "";
        var templateId = configuration.GetValue<string>("GOVUKNotify:RequestReviewApplicationEmailTemplateId") ?? "";
        var supportAdminEmailAddress = configuration.GetValue<string>("GOVUKNotify:SupportAdminEmailAddress") ?? "";

        var missingConfigs = new List<string>();

        if (string.IsNullOrEmpty(baseAppUrl))
            missingConfigs.Add("OrganisationAppUrl");
        if (string.IsNullOrEmpty(templateId))
            missingConfigs.Add("GOVUKNotify:RequestReviewApplicationEmailTemplateId");
        if (string.IsNullOrEmpty(supportAdminEmailAddress))
            missingConfigs.Add("GOVUKNotify:SupportAdminEmailAddress");

        if (missingConfigs.Count != 0)
        {
            logger.LogError(new Exception("Unable to send email to support admin"), $"Missing configuration keys: {string.Join(", ", missingConfigs)}. Unable to send email to support admin.");
            return;
        }

        var requestLink = new Uri(new Uri(baseAppUrl), $"/support/organisation/{organisation.Guid}/approval").ToString();

        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = supportAdminEmailAddress,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string>
            {
                { "org_name", organisation.Name },
                { "request_link", requestLink }
            }
        };

        try
        {
            await govUKNotifyApiClient.SendEmail(emailRequest);
        }
        catch
        {
            return;
        }
    }

    private async Task ValidateIdentifierIsNotKnownToUs(OrganisationIdentifier identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier.Id))
        {
            throw new InvalidUpdateOrganisationCommand.MissingIdentifierNumber();
        }

        var organisationIdentifier = await organisationRepository.FindByIdentifier(identifier.Scheme, identifier.Id);
        if (organisationIdentifier != null)
        {
            throw new InvalidUpdateOrganisationCommand.IdentiferNumberAlreadyExists();
        }
    }

    private void RemoveIdentifier(OrganisationInformation.Persistence.Organisation organisation,
        OrganisationInformation.Persistence.Identifier identifierToRemove)
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
            throw new InvalidUpdateOrganisationCommand.NoPrimaryIdentifier();
        }
    }
}