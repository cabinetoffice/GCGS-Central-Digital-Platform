using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using PersistenceOrganisation = CO.CDP.OrganisationInformation.Persistence.Organisation;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateSupplierInformationUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
            : IUseCase<(Guid organisationId, UpdateSupplierInformation updateSupplierInformation), bool>
{
    public async Task<bool> Execute((Guid organisationId, UpdateSupplierInformation updateSupplierInformation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        if (organisation.SupplierInfo == null)
        {
            throw new SupplierInfoNotExistException($"Supplier information for organisation {command.organisationId} not exist.");
        }

        var updateObject = command.updateSupplierInformation.SupplierInformation;

        switch (command.updateSupplierInformation.Type)
        {
            case SupplierInformationUpdateType.SupplierType:
                if (updateObject.SupplierType == null)
                {
                    throw new InvalidUpdateSupplierInformationCommand("Missing supplier type.");
                }
                organisation.SupplierInfo.SupplierType = updateObject.SupplierType;
                break;

            case SupplierInformationUpdateType.CompletedWebsiteAddress:
                organisation.SupplierInfo.CompletedWebsiteAddress = true;
                break;

            case SupplierInformationUpdateType.CompletedEmailAddress:
                organisation.SupplierInfo.CompletedEmailAddress = true;
                break;

            case SupplierInformationUpdateType.TradeAssurance:
                if (updateObject.TradeAssurance != null)
                {
                    var existingTradeAssurance = !updateObject.TradeAssurance.Id.HasValue ? null :
                        organisation.SupplierInfo.TradeAssurances.FirstOrDefault(ta => ta.Guid == updateObject.TradeAssurance.Id);
                    if (existingTradeAssurance != null)
                    {
                        existingTradeAssurance.AwardedByPersonOrBodyName = updateObject.TradeAssurance.AwardedByPersonOrBodyName;
                        existingTradeAssurance.ReferenceNumber = updateObject.TradeAssurance.ReferenceNumber;
                        existingTradeAssurance.DateAwarded = updateObject.TradeAssurance.DateAwarded;
                    }
                    else
                    {
                        organisation.SupplierInfo.TradeAssurances.Add(
                            mapper.Map<PersistenceOrganisation.TradeAssurance>(updateObject.TradeAssurance));
                    }
                }
                organisation.SupplierInfo.CompletedTradeAssurance = true;
                break;

            case SupplierInformationUpdateType.LegalForm:
                if (updateObject.LegalForm == null)
                {
                    throw new InvalidUpdateSupplierInformationCommand("Missing legal form.");
                }
                organisation.SupplierInfo.LegalForm = mapper.Map<PersistenceOrganisation.LegalForm>(updateObject.LegalForm);
                organisation.SupplierInfo.CompletedLegalForm = true;
                break;

            case SupplierInformationUpdateType.OperationType:
                if (updateObject.OperationTypes == null || !updateObject.OperationTypes.Any())
                {
                    throw new InvalidUpdateSupplierInformationCommand("Missing operation types.");
                }
                if (updateObject.OperationTypes.Contains(OperationType.None) && updateObject.OperationTypes.Count > 1)
                {
                    throw new InvalidUpdateSupplierInformationCommand("When 'None' is specified, it must be the only operation type.");
                }
                organisation.SupplierInfo.OperationTypes = updateObject.OperationTypes;
                organisation.SupplierInfo.CompletedOperationType = true;
                break;

            case SupplierInformationUpdateType.Qualification:
                if (updateObject.Qualification != null)
                {
                    var existingQualification = !updateObject.Qualification.Id.HasValue ? null :
                        organisation.SupplierInfo.Qualifications.FirstOrDefault(ta => ta.Guid == updateObject.Qualification.Id);
                    if (existingQualification != null)
                    {
                        existingQualification.AwardedByPersonOrBodyName = updateObject.Qualification.AwardedByPersonOrBodyName;
                        existingQualification.Name = updateObject.Qualification.Name;
                        existingQualification.DateAwarded = updateObject.Qualification.DateAwarded;
                    }
                    else
                    {
                        organisation.SupplierInfo.Qualifications.Add(
                            mapper.Map<PersistenceOrganisation.Qualification>(updateObject.Qualification));
                    }
                }
                organisation.SupplierInfo.CompletedQualification = true;
                break;
            default:
                throw new InvalidUpdateSupplierInformationCommand("Unknown supplier information update type.");
        }

        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}