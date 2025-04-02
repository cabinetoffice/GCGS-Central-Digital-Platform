using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using FormQuestionType = CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;

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

        if (org.Type == OrganisationType.InformalConsortium)
        {
            await AddConsortiumOrganisationSharedConsent(sharecode, supplierInformation);
        }

        InsertFileDocumentUri(sharedConsent, supplierInformation, sharecode);

        return supplierInformation;
    }

    private async Task AddConsortiumOrganisationSharedConsent(string parentShareCode, SupplierInformation supplierInformation)
    {
        var shareCodes = await shareCodeRepository.GetConsortiumOrganisationsShareCode(parentShareCode);

        foreach (var shareCode in shareCodes)
        {
            var sharedConsent = await shareCodeRepository.GetByShareCode(shareCode)
                                ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);
            var org = sharedConsent.Organisation;

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

    private void InsertFileDocumentUri(SharedConsentNonEf sharedConsent, SupplierInformation supplierInformation, string sharecode)
    {
        var dataSharingApiUrl = configuration["DataSharingApiUrl"]
                    ?? throw new Exception("Missing configuration key: DataSharingApiUrl.");

        var questions = sharedConsent.AnswerSets.SelectMany(s => s.Section.Questions);

        foreach (var answerSet in supplierInformation.SupplierInformationData.AnswerSets)
        {
            foreach (var answer in answerSet.Answers)
            {
                if (!string.IsNullOrWhiteSpace(answer.TextValue)
                    && questions.Any(q => q.Type == FormQuestionType.FileUpload && q.Name == answer.QuestionName))
                {
                    answer.DocumentUri = new Uri(new Uri(dataSharingApiUrl), $"/share/data/{sharecode}/document/{answer.TextValue}");
                    answer.TextValue = null;
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
                Name = string.Format($"{x.IndividualOrTrust!.FirstName} {x.IndividualOrTrust.LastName}"),
                Relationship = x.IndividualOrTrust.Category.ToString(),
                Uri = null,
                Roles = sharedConsent.Organisation.Roles,
                Details = new AssociatedPersonDetails
                {
                    FirstName = x.IndividualOrTrust.FirstName,
                    LastName = x.IndividualOrTrust.LastName,
                    DateOfBirth = x.IndividualOrTrust.DateOfBirth,
                    Nationality = x.IndividualOrTrust.Nationality,
                    ResidentCountry = x.IndividualOrTrust.ResidentCountry,
                    ControlCondition = x.IndividualOrTrust.ControlCondition,
                    ConnectedType = x.IndividualOrTrust.ConnectedType,
                    Addresses = ToAddress(x.Addresses),
                    RegisteredDate = x.RegisteredDate,
                    RegistrationAuthority = x.RegisterName,
                    HasCompanyHouseNumber = x.HasCompanyHouseNumber,
                    CompanyHouseNumber = x.CompanyHouseNumber,
                    OverseasCompanyNumber = x.OverseasCompanyNumber,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                }
            }).ToList();
    }

    private static ICollection<OrganisationReference> AdditionalEntities(SharedConsentNonEf sharedConsent)
    {
        return sharedConsent.Organisation.ConnectedEntities
            .Where(ce => ce.EntityType == ConnectedEntityType.Organisation && ce.Organisation != null)
            .Select(x => new OrganisationReference
            {
                Id = x.Guid,
                Name = x.Organisation!.Name,
                Roles = [],
                Uri = null,
                Details = new OrganisationReferenceDetails
                {
                    Category = x.Organisation.Category,
                    InsolvencyDate = x.Organisation.InsolvencyDate,
                    RegisteredLegalForm = x.Organisation.RegisteredLegalForm,
                    LawRegistered = x.Organisation.LawRegistered,
                    ControlCondition = x.Organisation.ControlCondition,
                    Addresses = ToAddress(x.Addresses),
                    RegisteredDate = x.RegisteredDate,
                    RegistrationAuthority = x.RegisterName,
                    HasCompanyHouseNumber = x.HasCompanyHouseNumber,
                    CompanyHouseNumber = x.CompanyHouseNumber,
                    OverseasCompanyNumber = x.OverseasCompanyNumber,
                    StartDate = x.StartDate,
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