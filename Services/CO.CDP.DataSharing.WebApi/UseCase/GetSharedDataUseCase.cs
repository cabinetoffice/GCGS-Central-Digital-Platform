using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;
using FormQuestionType = CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;

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

        var associatedPersons = await AssociatedPersons(sharedConsent);
        var additionalEntities = await AdditionalEntities(sharedConsent);
        var details = await GetDetails(sharedConsent);

        var supplierInformation = mapper.Map<SupplierInformation>(sharedConsent, opts =>
        {
            opts.Items["AssociatedPersons"] = associatedPersons;
            opts.Items["AdditionalParties"] = new List<OrganisationReference>();
            opts.Items["AdditionalEntities"] = additionalEntities;

            opts.Items["Details"] = details;
        });

        InsertFileDocumentUri(sharedConsent, supplierInformation, sharecode);

        return supplierInformation;
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
            Scale = operationTypes.Contains(OperationType.SmallOrMediumSized) ? "small" : "large",
            Vcse = operationTypes.Contains(OperationType.NonGovernmental),
            ShelteredWorkshop = operationTypes.Contains(OperationType.SupportedEmploymentProvider),
            PublicServiceMissionOrganization = operationTypes.Contains(OperationType.PublicService)
        };
    }

    private async Task<ICollection<AssociatedPerson>> AssociatedPersons(SharedConsent sharedConsent)
    {
        var associatedPersons = await organisationRepository.GetConnectedIndividualTrusts(sharedConsent.OrganisationId);
        return associatedPersons.Select(x => new AssociatedPerson
        {
            Id = x.Guid,
            Name = string.Format($"{x.IndividualOrTrust?.FirstName} {x.IndividualOrTrust?.LastName}"),
            Relationship = x.IndividualOrTrust?.Category.ToString() ?? string.Empty,
            Uri = null,
            Roles = []
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