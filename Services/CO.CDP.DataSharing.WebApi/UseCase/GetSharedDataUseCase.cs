using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using FormQuestionType = CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;
using ConnectedEntityIndividualAndTrustCategoryType = CO.CDP.OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataUseCase(
    CO.CDP.OrganisationInformation.Persistence.IShareCodeRepository shareCodeRepository,
    IMapper mapper,
    IConfiguration configuration)
    : IUseCase<string, SupplierInformation?>
{
    public async Task<SupplierInformation?> Execute(string sharecode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(sharecode)
                            ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);

        var org = sharedConsent.Organisation;
        var associatedPersons = AssociatedPersons(sharedConsent);
        var additionalEntities = AdditionalEntities(sharedConsent);
        var details = GetDetails(sharedConsent);

        var supplierInformation = mapper.Map<SupplierInformation>(sharedConsent, opts =>
        {
            opts.Items["AssociatedPersons"] = associatedPersons;
            opts.Items["AdditionalEntities"] = additionalEntities;
            opts.Items["Details"] = details;
        });

        supplierInformation.SupplierInformationData.AnswerSets.ForEach(answerSet => answerSet.OrganisationId = org.Guid);
        supplierInformation.SupplierInformationData.Questions.ForEach(question => question.OrganisationId = org.Guid);

        var organisationShareCodeMap = new Dictionary<Guid, string>();
        organisationShareCodeMap[sharedConsent.Organisation.Guid] = sharecode;

        if (org.Type == OrganisationType.InformalConsortium)
        {
            await AddConsortiumOrganisationSharedConsent(sharecode, supplierInformation, organisationShareCodeMap);
        }

        InsertFileDocumentUri(supplierInformation, organisationShareCodeMap);

        return supplierInformation;
    }

    private async Task AddConsortiumOrganisationSharedConsent(string parentShareCode, SupplierInformation supplierInformation, Dictionary<Guid, string> organisationShareCodeMap)
    {
        var shareCodes = await shareCodeRepository.GetConsortiumOrganisationsShareCode(parentShareCode);

        foreach (var shareCode in shareCodes)
        {
            var sharedConsent = await shareCodeRepository.GetByShareCode(shareCode)
                                ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);
            var org = sharedConsent.Organisation;

            organisationShareCodeMap[org.Guid] = shareCode;

            var answerSets = mapper.Map<List<FormAnswerSet>>(sharedConsent.AnswerSets);
            answerSets.ForEach(answerSet => answerSet.OrganisationId = org.Guid);
            supplierInformation.SupplierInformationData.AnswerSets.AddRange(answerSets);

            var questions = mapper.Map<List<FormQuestion>>(sharedConsent.AnswerSets
                .SelectMany(a => a.Section.Questions.Where(x => x.Type != FormQuestionType.NoInput && x.Type != FormQuestionType.CheckYourAnswers))
                .DistinctBy(q => q.Id));
            questions.ForEach(question => question.OrganisationId = org.Guid);
            supplierInformation.SupplierInformationData.Questions.AddRange(questions);

            supplierInformation.AdditionalParties.Add(new OrganisationReference
            {
                Id = org.Guid,
                Name = org.Name,
                Roles = org.Roles,
                Uri = null,
                ShareCode = new OrganisationReferenceShareCode
                {
                    Value = shareCode,
                    SubmittedAt = sharedConsent.SubmittedAt!.Value
                }
            });
        }
    }

    private void InsertFileDocumentUri(SupplierInformation supplierInformation, Dictionary<Guid, string> organisationShareCodeMap)
    {
        var dataSharingApiUrl = configuration["DataSharingApiUrl"]
                    ?? throw new Exception("Missing configuration key: DataSharingApiUrl.");

        var questions = supplierInformation.SupplierInformationData.Questions;

        foreach (var answerSet in supplierInformation.SupplierInformationData.AnswerSets)
        {
            foreach (var answer in answerSet.Answers)
            {
                if (!string.IsNullOrWhiteSpace(answer.TextValue)
                    && questions.Any(q => q.Type == Model.FormQuestionType.FileUpload && q.Name == answer.QuestionName))
                {
                    var organisationId = answerSet.OrganisationId;
                    if (organisationId != null)
                    {
                        var shareCode = organisationShareCodeMap[organisationId.Value];

                        answer.DocumentUri = new Uri(new Uri(dataSharingApiUrl), $"/share/data/{shareCode}/document/{answer.TextValue}");
                        answer.TextValue = null;
                    }
                }
            }
        }
    }


    private Details GetDetails(SharedConsentNonEf sharedConsent)
    {
        var legalForm = sharedConsent.Organisation.SupplierInfo?.LegalForm;
        var operationTypes = sharedConsent.Organisation.SupplierInfo?.OperationTypes;

        return new Details
        {
            LegalForm = legalForm != null ? mapper.Map<LegalForm>(legalForm) : null,
            Scale = (operationTypes != null && operationTypes.Contains(OperationType.SmallOrMediumSized)) ? "small"
                : ((operationTypes == null || operationTypes.Count == 0) ? null : "large"),
            Vcse = (operationTypes != null && operationTypes.Count > 0) ? operationTypes.Contains(OperationType.NonGovernmental) : (bool?)null,
            ShelteredWorkshop = (operationTypes != null && operationTypes.Count > 0) ? operationTypes.Contains(OperationType.SupportedEmploymentProvider) : (bool?)null,
            PublicServiceMissionOrganization = (operationTypes != null && operationTypes.Count > 0) ? operationTypes.Contains(OperationType.PublicService) : (bool?)null
        };
    }

    private static ICollection<AssociatedPerson> AssociatedPersons(SharedConsentNonEf sharedConsent)
    {
        return sharedConsent.Organisation.ConnectedEntities
            .Where(ce => ce.EntityType != ConnectedEntityType.Organisation && ce.IndividualOrTrust != null)
            .Select(x => new AssociatedPerson
            {
                Id = x.Guid,
                EntityType = x.IndividualOrTrust!.ConnectedType,
                Relationship = ToAssociatedRelationship(x.IndividualOrTrust!.Category),
                FirstName = x.IndividualOrTrust.FirstName,
                LastName = x.IndividualOrTrust.LastName,
                DateOfBirth = x.IndividualOrTrust.DateOfBirth,
                Nationality = x.IndividualOrTrust.Nationality,
                ResidentCountry = x.IndividualOrTrust.ResidentCountry,
                ControlCondition = x.IndividualOrTrust.ControlCondition,
                Addresses = ToAddress(x.Addresses),
                RegistrationAuthority = x.RegisterName,
                RegisteredDate = x.RegisteredDate,
                HasCompanyHouseNumber = x.HasCompanyHouseNumber,
                CompanyHouseNumber = x.CompanyHouseNumber,
                OverseasCompanyNumber = x.OverseasCompanyNumber,
                Period = new AssociatedPeriod
                {
                    EndDate = x.EndDate
                }
            }).ToList();
    }

    private static AssociatedRelationship ToAssociatedRelationship(ConnectedEntityIndividualAndTrustCategoryType relationship)
    {
        return relationship switch
        {
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv => AssociatedRelationship.PersonWithSignificantControlForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForIndiv => AssociatedRelationship.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForIndiv => AssociatedRelationship.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust => AssociatedRelationship.PersonWithSignificantControlForTrust,
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForTrust => AssociatedRelationship.DirectorOrIndividualWithTheSameResponsibilitiesForTrust,
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForTrust => AssociatedRelationship.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust,
            _ => throw new Exception($"{nameof(ConnectedEntityIndividualAndTrustCategoryType)} enum to {nameof(AssociatedRelationship)} enum does not exists."),
        };
    }

    private static ICollection<AssociatedEntity> AdditionalEntities(SharedConsentNonEf sharedConsent)
    {
        return sharedConsent.Organisation.ConnectedEntities
            .Where(ce => ce.EntityType == ConnectedEntityType.Organisation && ce.Organisation != null)
            .Select(x => new AssociatedEntity
            {
                Id = x.Guid,
                Name = x.Organisation!.Name,
                Category = x.Organisation.Category,
                InsolvencyDate = x.Organisation.InsolvencyDate,
                RegisteredLegalForm = x.Organisation.RegisteredLegalForm,
                LawRegistered = x.Organisation.LawRegistered,
                ControlCondition = x.Organisation.ControlCondition,
                Addresses = ToAddress(x.Addresses),
                RegistrationAuthority = x.RegisterName,
                RegisteredDate = x.RegisteredDate,
                HasCompanyHouseNumber = x.HasCompanyHouseNumber,
                CompanyHouseNumber = x.CompanyHouseNumber,
                OverseasCompanyNumber = x.OverseasCompanyNumber,
                Period = new AssociatedPeriod
                {
                    EndDate = x.EndDate
                }
            }).ToList();
    }
    private static IEnumerable<Address> ToAddress(ICollection<AddressNonEf> addresses)
        => addresses.Select(a => new Address
        {
            StreetAddress = a.StreetAddress,
            Locality = a.Locality,
            Region = a.Region,
            CountryName = a.CountryName,
            Country = a.Country,
            PostalCode = a.PostalCode,
            Type = a.Type ?? AddressType.Registered
        });
}