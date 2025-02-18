using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FormQuestionType = CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataUseCase(
    IShareCodeRepository shareCodeRepository,
    IOrganisationRepository organisationRepository,
    IMapper mapper,
    IConfiguration configuration)
    : IUseCase<string, SupplierInformation?>
{
    public async Task<SupplierInformation?> Execute(string sharecode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(sharecode)
                            ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);

        var org = sharedConsent.Organisation;
        var associatedPersons = await AssociatedPersons(sharedConsent);
        var additionalEntities = await AdditionalEntities(sharedConsent);
        var details = await GetDetails(sharedConsent);

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

    private void InsertFileDocumentUri(SharedConsent sharedConsent, SupplierInformation supplierInformation, string sharecode)
    {
        var dataSharingApiUrl = configuration["DataSharingApiUrl"]
                    ?? throw new Exception("Missing configuration key: DataSharingApiUrl.");

        var questions = sharedConsent.Form.Sections.SelectMany(s => s.Questions);

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

    private async Task<Details> GetDetails(SharedConsent sharedConsent)
    {
        var legalForm = await organisationRepository.GetLegalForm(sharedConsent.OrganisationId);
        var operationTypes = await organisationRepository.GetOperationTypes(sharedConsent.OrganisationId);

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

    private async Task<ICollection<AssociatedPerson>> AssociatedPersons(SharedConsent sharedConsent)
    {
        var individuals = await organisationRepository.GetConnectedIndividualTrusts(sharedConsent.OrganisationId);
        var trustsOrTrustees = await organisationRepository.GetConnectedTrustsOrTrustees(sharedConsent.OrganisationId);

        return individuals.Union(trustsOrTrustees).Select(x => new AssociatedPerson
        {
            Id = x.Guid,
            Name = string.Format($"{x.IndividualOrTrust?.FirstName} {x.IndividualOrTrust?.LastName}"),
            Relationship = x.IndividualOrTrust?.Category.ToString() ?? string.Empty,
            Uri = null,
            Roles = x.SupplierOrganisation.Roles
        }).ToList();
    }

    private async Task<ICollection<OrganisationReference>> AdditionalEntities(SharedConsent sharedConsent)
    {
        var additionalEntities = await organisationRepository.GetConnectedOrganisations(sharedConsent.OrganisationId);

        return additionalEntities.Select(x => new OrganisationReference
        {
            Id = x.Guid,
            Name = x.Organisation?.Name ?? string.Empty,
            Roles = [],
            Uri = null
        }).ToList();
    }
}