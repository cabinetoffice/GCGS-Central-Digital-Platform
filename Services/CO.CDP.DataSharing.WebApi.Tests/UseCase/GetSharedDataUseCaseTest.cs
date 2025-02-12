using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.Tests.AutoMapper;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Address = CO.CDP.OrganisationInformation.Address;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class GetSharedDataUseCaseTest : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IConfiguration> _configuration = new();
    private readonly GetSharedDataUseCase _useCase;

    public GetSharedDataUseCaseTest(AutoMapperFixture mapperFixture)
    {
        _useCase = new GetSharedDataUseCase(_shareCodeRepository.Object, _organisationRepository.Object,
            mapperFixture.Mapper, _configuration.Object);
    }

    [Fact]
    public async Task ThrowsShareCodeNotFoundException_When_NotFound()
    {
        var response = async () => await _useCase.Execute(("dummy_code"));

        await response.Should().ThrowAsync<ShareCodeNotFoundException>();
    }

    [Fact]
    public async Task ThrowsException_WhenDataSharingApiUrl_NotConfigured()
    {
        var (shareCode, _, organisationGuid, _) = SetupTestData();

        var response = async () => await _useCase.Execute((shareCode));

        await response.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ItReturnsMappedSupplierInformationWhenSharedConsentIsFound()
    {
        var (shareCode, _, organisationGuid, _) = SetupTestData();

        _configuration.Setup(c => c["DataSharingApiUrl"]).Returns("https://localhost");

        var result = await _useCase.Execute((shareCode));
        result.Should().NotBeNull();

        AssertBasicInformation(result, organisationGuid);
        AssertAddress(result?.Address);
        AssertAssociatedPersons(result?.AssociatedPersons);
        AssertAdditionalEntities(result?.AdditionalEntities);
        AssertIdentifier(result?.Identifier);
        AssertDetails(result?.Details);
        AssertAdditionalParties(result?.AdditionalParties);
        AssertAdditionalIdentifiers(result?.AdditionalIdentifiers);
        AssertContactPoint(result?.ContactPoint);
        AssertRoles(result?.Roles);
        AssertSupplierInformationData(result?.SupplierInformationData);
    }

    [Fact]
    public async Task Execute_ShouldAddConsortiumOrganisationSharedConsent_WhenOrganisationIsInformalConsortium()
    {
        _configuration.Setup(c => c["DataSharingApiUrl"]).Returns("https://localhost");

        var consortiumShareCode = "consortium-sharecode";
        var subSharecode1 = "sub-sharecode-1";
        var subSharecode2 = "sub-sharecode-2";
        var consortiumOrgsShareCodes = new List<string> { "sub-sharecode-1", "sub-sharecode-2" };

        (string _, int organisationId, Guid organisationGuid, Guid formId) = SetupTestData(consortiumShareCode, true);
        (string _, int organisationId1, Guid organisationGuid1, Guid formId1) = SetupTestData(subSharecode1);
        (string _, int organisationId2, Guid organisationGuid2, Guid formId2) = SetupTestData(subSharecode2);

        _shareCodeRepository.Setup(repo => repo.GetConsortiumOrganisationsShareCode(consortiumShareCode))
            .ReturnsAsync(consortiumOrgsShareCodes);

        var result = await _useCase.Execute(consortiumShareCode);

        result.Should().NotBeNull();
        result!.SupplierInformationData.AnswerSets.Should().NotBeEmpty();
        result.SupplierInformationData.Questions.Should().NotBeEmpty();
        result.AdditionalParties.Should().HaveCount(2);

        result.SupplierInformationData.AnswerSets.Where(a => a.OrganisationId == organisationGuid).Should().HaveCountGreaterThan(1);
        result.SupplierInformationData.AnswerSets.Where(a => a.OrganisationId == organisationGuid1).Should().HaveCountGreaterThan(1);
        result.SupplierInformationData.AnswerSets.Where(a => a.OrganisationId == organisationGuid2).Should().HaveCountGreaterThan(1);

        result.SupplierInformationData.Questions.Where(a => a.OrganisationId == organisationGuid).Should().HaveCountGreaterThan(1);
        result.SupplierInformationData.Questions.Where(a => a.OrganisationId == organisationGuid1).Should().HaveCountGreaterThan(1);
        result.SupplierInformationData.Questions.Where(a => a.OrganisationId == organisationGuid2).Should().HaveCountGreaterThan(1);
    }

    private (string shareCode, int organisationId, Guid organisationGuid, Guid formId) SetupTestData(
        string shareCode = "valid-sharecode", bool isConsortium = false)
    {
        int organisationId = 1;
        Guid organisationGuid = Guid.NewGuid();
        Guid formId = Guid.NewGuid();

        var organisation = EntityFactory.GivenOrganisation(organisationGuid, name: "Test Organisation");

        var sharedConsent = EntityFactory.GetSharedConsent(organisationId, organisationGuid, formId);
        sharedConsent.ShareCode = shareCode;
        if (isConsortium) sharedConsent.Organisation.Type = OrganisationType.InformalConsortium;

        _shareCodeRepository.Setup(repo => repo.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

        var mockIndividuals = EntityFactory.GetMockIndividuals();
        var mockAdditionalEntities = EntityFactory.GetMockAdditionalEntities();
        var mockTrustOrTrustees = EntityFactory.GetMockTrustsOrTrustees();
        var mockLegalForm = EntityFactory.GetLegalForm();
        var mockOperationTypes = EntityFactory.GetOperationTypes();

        _organisationRepository.Setup(repo => repo.GetConnectedIndividualTrusts(organisationId))
                               .ReturnsAsync(mockIndividuals);

        _organisationRepository.Setup(repo => repo.GetConnectedOrganisations(organisationId))
                               .ReturnsAsync(mockAdditionalEntities);

        _organisationRepository.Setup(repo => repo.GetConnectedTrustsOrTrustees(organisationId))
                                .ReturnsAsync(mockTrustOrTrustees);

        _organisationRepository.Setup(repo => repo.GetLegalForm(organisationId))
                               .ReturnsAsync(mockLegalForm);

        _organisationRepository.Setup(repo => repo.GetOperationTypes(organisationId))
                               .ReturnsAsync(mockOperationTypes);

        return (shareCode, organisationId, organisationGuid, formId);
    }

    private void AssertBasicInformation(SupplierInformation? result, Guid organisationGuid)
    {
        result.Should().NotBeNull();
        result?.Id.Should().Be(organisationGuid);
        result?.Name.Should().Be("Test Organisation");
    }

    private void AssertAddress(Address? address)
    {
        address.Should().NotBeNull();
        address?.StreetAddress.Should().Be("1234 Default St");
        address?.Locality.Should().Be("Default City");
        address?.Region.Should().Be("Default Region");
        address?.PostalCode.Should().Be("EX1 1EX");
        address?.CountryName.Should().Be("Example Country");
        address?.Country.Should().Be("EX");
    }

    private void AssertAssociatedPersons(IEnumerable<AssociatedPerson>? associatedPersons)
    {
        associatedPersons.Should().NotBeNull();
        associatedPersons.Should().HaveCount(2);
        associatedPersons?.First().Name.Should().Be("John Doe");
        associatedPersons?.Should().Contain(x => x.Name == "John Smith");
    }

    private void AssertAdditionalEntities(IEnumerable<OrganisationReference>? additionalEntities)
    {
        additionalEntities.Should().NotBeNull();
        additionalEntities.Should().HaveCount(EntityFactory.GetMockAdditionalEntities().Count);
    }

    private void AssertIdentifier(OrganisationInformation.Identifier? identifier)
    {
        identifier.Should().NotBeNull();
        identifier?.LegalName.Should().Be("DefaultLegalName");
    }

    private void AssertAdditionalParties(IEnumerable<OrganisationReference>? additionalParties)
    {
        additionalParties.Should().NotBeNull();
    }

    private void AssertAdditionalIdentifiers(IEnumerable<OrganisationInformation.Identifier>? additionalIdentifiers)
    {
        additionalIdentifiers.Should().NotBeNull();
        additionalIdentifiers.Should().HaveCount(1);

        var identifier = additionalIdentifiers?.First();
        identifier?.Scheme.Should().Be("GB-COH");
        identifier?.LegalName.Should().Be("AnotherLegalName");
        identifier?.Uri.Should().Be(new Uri("http://example.com"));
    }

    private void AssertContactPoint(ContactPoint? contactPoint)
    {
        contactPoint.Should().NotBeNull();
        contactPoint!.Name.Should().Be("Default Contact");
        contactPoint.Email.Should().Be("contact@default.org");
        contactPoint.Telephone.Should().Be("123-456-7890");
        contactPoint.Url.Should().Be("https://contact.default.org");
    }

    private void AssertRoles(IEnumerable<PartyRole>? roles)
    {
        roles.Should().NotBeNull();
        roles.Should().HaveCount(1);

        var roleStrings = roles!.Select(r => r.AsCode()).ToList();
        roleStrings.Should().Contain("buyer");
    }

    private void AssertDetails(Details? details)
    {
        details.Should().NotBeNull();
        details?.LegalForm?.RegisteredUnderAct2006.Should().Be(false);
        details?.LegalForm?.RegisteredLegalForm.Should().Be("Registered Legal Form 1");
        details?.LegalForm?.LawRegistered.Should().Be("Law Registered 1");
        details?.LegalForm?.RegistrationDate.Should().Be(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"));
        details?.Scale.Should().Be("small");
        details?.Vcse.Should().Be(false);
        details?.ShelteredWorkshop.Should().Be(false);
        details?.PublicServiceMissionOrganization.Should().Be(false);
    }

    private void AssertSupplierInformationData(SupplierInformationData? supplierInformationData)
    {
        supplierInformationData.Should().NotBeNull();
        supplierInformationData?.Form.Name.Should().Be("Standard Questions");

        supplierInformationData?.AnswerSets.Should().NotBeNull();
        supplierInformationData?.AnswerSets.Should().HaveCount(2);

        supplierInformationData?.AnswerSets.First().Answers.Should().NotBeNull();
        supplierInformationData?.AnswerSets.First().Answers.Should().HaveCount(3);

        supplierInformationData?.AnswerSets.First().SectionName.Should().NotBeNull();
        supplierInformationData?.AnswerSets.First().SectionName.Should().Be("Localized string");

        supplierInformationData?.AnswerSets.First().Answers.First().QuestionName.Should().NotBeNull();

        supplierInformationData?.Questions.Should().NotBeNull();
        supplierInformationData?.Questions.First().SectionName.Should().Be("Localized string");
        supplierInformationData?.Questions.First().Title.Should().Be("Localized string");
        supplierInformationData?.Questions.First().Text.Should().Be("Localized string");

        supplierInformationData.Should().NotBeNull();
        supplierInformationData!.Form.Name.Should().Be("Standard Questions");

        supplierInformationData.AnswerSets.Should().NotBeNull();
        supplierInformationData.AnswerSets.Should().HaveCount(2);

        foreach (var answerSet in supplierInformationData.AnswerSets)
        {
            answerSet.Answers.Should().NotBeNull();

            foreach (var answer in answerSet.Answers)
            {
                AssertAnswerDetails(answer);
            }
        }

        supplierInformationData.Questions.Should().NotBeNull();

        foreach (var question in supplierInformationData.Questions)
        {
            AssertQuestionDetails(question);
        }
    }

    private void AssertAnswerDetails(FormAnswer? answer)
    {
        answer.Should().NotBeNull();
        answer!.QuestionName.Should().NotBeNull();

        if (!string.IsNullOrEmpty(answer.TextValue))
            answer.TextValue.Should().Be("Compliance confirmed through third-party audit.");

        if (answer.QuestionName == "_Section03")
        {
            answer.TextValue.Should().BeNull();
            answer.DocumentUri.Should().Be("https://localhost/share/data/valid-sharecode/document/a_dummy_file.pdf");
        }

        if (answer.BoolValue.HasValue)
            answer.BoolValue.Should().BeTrue();

        if (answer.DateValue.HasValue)
        {
            var actualDate = answer?.DateValue;
            var expectedDate = DateOnly.FromDateTime(DateTime.Now);

            actualDate.Should().Be(expectedDate);
        }

    }
    private void AssertQuestionDetails(FormQuestion? question)
    {
        question.Should().NotBeNull();
        question!.Title.Should().NotBeNullOrWhiteSpace();
        question?.Name.Should().NotBeNullOrWhiteSpace();

        question?.Type.Should().NotBe(FormQuestionType.None);
        question?.IsRequired.Should().BeTrue();
    }

}