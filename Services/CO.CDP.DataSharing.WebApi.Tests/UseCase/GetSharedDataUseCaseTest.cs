using CO.CDP.Authentication;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.Tests.AutoMapper;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Address = CO.CDP.OrganisationInformation.Address;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class GetSharedDataUseCaseTest : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IClaimService> _claimService = new();
    private readonly GetSharedDataUseCase _useCase;

    public GetSharedDataUseCaseTest(AutoMapperFixture mapperFixture)
    {
        var organisationId = Guid.NewGuid();
        _claimService.Setup(c => c.GetOrganisationId()).Returns(organisationId);
        _shareCodeRepository.Setup(s => s.OrganisationShareCodeExistsAsync(organisationId, It.IsAny<string>())).ReturnsAsync(true);

        _useCase = new GetSharedDataUseCase(_shareCodeRepository.Object, _organisationRepository.Object, _claimService.Object, mapperFixture.Mapper);
    }

    [Fact]
    public async Task ThrowsUserUnauthorizedException_WhenRequestedUserOrganisationAndShareCodeRequestedNotFound()
    {
        var shareCode = "dummy_code";

        var organisationId = Guid.NewGuid();
        _claimService.Setup(c => c.GetOrganisationId()).Returns(organisationId);
        _shareCodeRepository.Setup(s => s.OrganisationShareCodeExistsAsync(organisationId, shareCode)).ReturnsAsync(false);

        var response = async () => await _useCase.Execute(shareCode);

        await response.Should().ThrowAsync<UserUnauthorizedException>();
    }

    [Fact]
    public async Task ThrowsUserUnauthorizedException_WhenRequestedUserOrganisationInNotInTheClaim()
    {
        _claimService.Setup(c => c.GetOrganisationId()).Returns((Guid?)null);

        var response = async () => await _useCase.Execute("dummy_code");

        await response.Should().ThrowAsync<UserUnauthorizedException>();
    }

    [Fact]
    public async Task ItReturnsMappedSupplierInformationWhenSharedConsentIsFound()
    {
        var (shareCode, _, organisationGuid, _) = SetupTestData();

        var result = await _useCase.Execute(shareCode);

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

    private (string shareCode, int organisationId, Guid organisationGuid, Guid formId) SetupTestData()
    {
        string shareCode = "valid-sharecode";
        int organisationId = 1;
        Guid organisationGuid = Guid.NewGuid();
        Guid formId = Guid.NewGuid();

        var organisation = EntityFactory.GivenOrganisation(organisationGuid, name: "Test Organisation");

        var sharedConsent = EntityFactory.GetSharedConsent(organisationId, organisationGuid, formId);
        sharedConsent.ShareCode = shareCode;

        _shareCodeRepository.Setup(repo => repo.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

        var mockAssociatedPersons = EntityFactory.GetMockAssociatedPersons();
        var mockAdditionalEntities = EntityFactory.GetMockAdditionalEntities();

        _organisationRepository.Setup(repo => repo.GetConnectedIndividualTrusts(organisationId))
                               .ReturnsAsync(mockAssociatedPersons);

        _organisationRepository.Setup(repo => repo.GetConnectedOrganisations(organisationId))
                               .ReturnsAsync(mockAdditionalEntities);

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
        associatedPersons.Should().HaveCount(1);
        associatedPersons?.First().Name.Should().Be("John Doe");
    }

    private void AssertAdditionalEntities(IEnumerable<OrganisationReference>? additionalEntities)
    {
        additionalEntities.Should().NotBeNull();
        additionalEntities.Should().HaveCount(EntityFactory.GetMockAdditionalEntities().Count);
    }

    private void AssertIdentifier(Identifier? identifier)
    {
        identifier.Should().NotBeNull();
        identifier?.LegalName.Should().Be("DefaultLegalName");
    }

    private void AssertAdditionalParties(IEnumerable<OrganisationReference>? additionalParties)
    {
        additionalParties.Should().NotBeNull();
    }

    private void AssertAdditionalIdentifiers(IEnumerable<Identifier>? additionalIdentifiers)
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

        supplierInformationData?.AnswerSets.First().Answers.First().QuestionName.Should().NotBeNull();

        supplierInformationData?.Questions.Should().NotBeNull();

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