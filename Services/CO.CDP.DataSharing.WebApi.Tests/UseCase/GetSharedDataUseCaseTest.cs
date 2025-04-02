using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.Tests.AutoMapper;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.Configuration;
using Moq;
using Address = CO.CDP.OrganisationInformation.Address;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class GetSharedDataUseCaseTest : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<OrganisationInformation.Persistence.IShareCodeRepository> _shareCodeRepository = new();
    private readonly Mock<IConfiguration> _configuration = new();
    private readonly GetSharedDataUseCase _useCase;

    public GetSharedDataUseCaseTest(AutoMapperFixture mapperFixture)
    {
        _useCase = new GetSharedDataUseCase(_shareCodeRepository.Object,
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

        result.AdditionalParties.First().ShareCode!.Value.Should().Be(subSharecode1);
        result.AdditionalParties.Last().ShareCode!.Value.Should().Be(subSharecode2);
    }

    private (string shareCode, int organisationId, Guid organisationGuid, Guid formId) SetupTestData(
        string shareCode = "valid-sharecode", bool isConsortium = false)
    {
        int organisationId = 1;
        Guid organisationGuid = new Guid("1db6029d-a077-475b-a6b8-80a79c824787");
        Guid formId = Guid.NewGuid();

        var sharedConsent = NonEfEntityFactory.GetSharedConsent(organisationGuid, formId);
        sharedConsent.ShareCode = shareCode;
        if (isConsortium) sharedConsent.Organisation.Type = OrganisationType.InformalConsortium;

        _shareCodeRepository.Setup(repo => repo.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

        var mockIndividuals = NonEfEntityFactory.GetMockIndividuals();
        var mockAdditionalEntities = NonEfEntityFactory.GetMockAdditionalEntities();
        var mockTrustOrTrustees = NonEfEntityFactory.GetMockTrustsOrTrustees();
        var mockLegalForm = NonEfEntityFactory.GetLegalForm();
        var mockOperationTypes = NonEfEntityFactory.GetOperationTypes();

        if (sharedConsent.Organisation != null)
        {
            if (sharedConsent.Organisation.SupplierInfo != null)
            {
                sharedConsent.Organisation.SupplierInfo.LegalForm = mockLegalForm;
                sharedConsent.Organisation.SupplierInfo.OperationTypes = [.. mockOperationTypes];
            }
            sharedConsent.Organisation.ConnectedEntities.AddRange(mockIndividuals);
            sharedConsent.Organisation.ConnectedEntities.AddRange(mockAdditionalEntities);
            sharedConsent.Organisation.ConnectedEntities.AddRange(mockTrustOrTrustees);
        }

        SetupConnectedEntityData(
            sharedConsent,
            organisationGuid,
            mockAdditionalEntities.First().Guid,
            mockIndividuals.First().Guid,
            mockTrustOrTrustees.First().Guid);

        return (shareCode, organisationId, organisationGuid, formId);
    }

    private static void SetupConnectedEntityData(
        OrganisationInformation.Persistence.NonEfEntities.SharedConsentNonEf sharedConsent,
        Guid organisationGuid,
        Guid connectedOrganisationGuid,
        Guid connectedIndividualGuid,
        Guid connectedTrusteeGuid)
    {
        var answerSet = sharedConsent.AnswerSets.First();
        var section = sharedConsent.AnswerSets.First().Section;

        var question = new OrganisationInformation.Persistence.NonEfEntities.FormQuestionNonEf
        {
            Id = 6,
            Description = "Exclusion applies to",
            Title = "Exclusion_Applies_To",
            Guid = Guid.NewGuid(),
            SortOrder = 7,
            Section = section,
            IsRequired = false,
            Name = "Exclusion_Applies_To",
            Options = new OrganisationInformation.Persistence.Forms.FormQuestionOptions
            {
                ChoiceProviderStrategy = "ExclusionAppliesToChoiceProviderStrategy"
            },
            Type = OrganisationInformation.Persistence.Forms.FormQuestionType.SingleChoice
        };
        answerSet.Section.Questions.Add(question);

        answerSet.Answers.Add(new OrganisationInformation.Persistence.NonEfEntities.FormAnswerNonEf
        {
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            JsonValue = $"{{\"id\": \"{organisationGuid}\", \"type\": \"organisation\"}}"
        });

        answerSet.Answers.Add(new OrganisationInformation.Persistence.NonEfEntities.FormAnswerNonEf
        {
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            JsonValue = $"{{\"id\": \"{connectedOrganisationGuid}\", \"type\": \"connected-entity\"}}"
        });

        answerSet.Answers.Add(new OrganisationInformation.Persistence.NonEfEntities.FormAnswerNonEf
        {
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            JsonValue = $"{{\"id\": \"{connectedIndividualGuid}\", \"type\": \"connected-entity\"}}"
        });

        answerSet.Answers.Add(new OrganisationInformation.Persistence.NonEfEntities.FormAnswerNonEf
        {
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            JsonValue = $"{{\"id\": \"{connectedTrusteeGuid}\", \"type\": \"connected-entity\"}}"
        });
    }

    private static void AssertBasicInformation(SupplierInformation? result, Guid organisationGuid)
    {
        result.Should().NotBeNull();
        result?.Id.Should().Be(organisationGuid);
        result?.Name.Should().Be("Test Organisation");
    }

    private static void AssertAddress(Address? address)
    {
        address.Should().NotBeNull();
        address?.StreetAddress.Should().Be("1234 Default St");
        address?.Locality.Should().Be("Default City");
        address?.Region.Should().Be("Default Region");
        address?.PostalCode.Should().Be("EX1 1EX");
        address?.CountryName.Should().Be("Example Country");
        address?.Country.Should().Be("EX");
    }

    private static void AssertAssociatedPersons(IEnumerable<AssociatedPerson>? associatedPersons)
    {
        associatedPersons.Should().NotBeNull();
        associatedPersons.Should().HaveCount(2);
        AssociatedPerson individual = associatedPersons!.First();

        individual.Name.Should().Be("John Doe");
        individual.Relationship.Should().Be("PersonWithSignificantControlForIndiv");
        individual.Roles.Should().Contain(PartyRole.Buyer);
        individual.Details.FirstName.Should().Be("John");
        individual.Details.LastName.Should().Be("Doe");
        individual.Details.DateOfBirth.Should().Be(DateTime.Today.AddYears(30));
        individual.Details.Nationality.Should().Be("British");
        individual.Details.ResidentCountry.Should().Be("United Kingdom");
        individual.Details.ControlCondition.Should()
            .ContainInConsecutiveOrder([ControlCondition.HasOtherSignificantInfluenceOrControl, ControlCondition.HasVotingRights]);
        individual.Details.ConnectedType.Should().Be(ConnectedPersonType.Individual);
        individual.Details.RegisteredDate.Should().Be(DateTime.Today.ToDateTimeOffset());
        individual.Details.RegistrationAuthority.Should().Be("Approved By Trade Association");
        individual.Details.HasCompanyHouseNumber.Should().Be(true);
        individual.Details.CompanyHouseNumber.Should().Be("TestOrg123");
        individual.Details.OverseasCompanyNumber.Should().Be("Oversears123");
        individual.Details.StartDate.Should().Be(DateTime.Today.AddDays(30).ToDateTimeOffset());
        individual.Details.EndDate.Should().Be(DateTime.Today.AddDays(5).ToDateTimeOffset());
        individual.Details.Addresses.Should().HaveCount(1);

        Address individualAddress = individual.Details.Addresses.First();
        individualAddress.StreetAddress.Should().Be("1234 Default St");
        individualAddress.Locality.Should().Be("Default City");
        individualAddress.Region.Should().Be("Default Region");
        individualAddress.PostalCode.Should().Be("EX1 1EX");
        individualAddress.CountryName.Should().Be("Example Country");
        individualAddress.Country.Should().Be("EX");
        individualAddress.Type.Should().Be(AddressType.Registered);

        AssociatedPerson trustee = associatedPersons!.Last();

        trustee.Name.Should().Be("John Smith");
        trustee.Relationship.Should().Be("PersonWithSignificantControlForTrust");
        trustee.Roles.Should().Contain(PartyRole.Buyer);
        trustee.Details.FirstName.Should().Be("John");
        trustee.Details.LastName.Should().Be("Smith");
        trustee.Details.ConnectedType.Should().Be(ConnectedPersonType.TrustOrTrustee);
    }

    private static void AssertAdditionalEntities(IEnumerable<OrganisationReference>? additionalEntities)
    {
        additionalEntities.Should().NotBeNull();
        additionalEntities.Should().HaveCount(EntityFactory.GetMockAdditionalEntities().Count);

        OrganisationReference org = additionalEntities!.First();

        org.Name.Should().Be("Acme Group Ltd");
        org.Details.Should().NotBeNull();
        org.Details!.Category.Should().Be(ConnectedOrganisationCategory.RegisteredCompany);
        org.Details.InsolvencyDate.Should().Be(DateTime.Today);
        org.Details.RegisteredLegalForm.Should().Be("Trade Association");
        org.Details.LawRegistered.Should().Be("Trade Law 2024");
        org.Details.ControlCondition.Should()
            .ContainInConsecutiveOrder([ControlCondition.CanAppointOrRemoveDirectors, ControlCondition.HasVotingRights, ControlCondition.OwnsShares]);

        org.Details.RegisteredDate.Should().Be(DateTime.Today.ToDateTimeOffset());
        org.Details.RegistrationAuthority.Should().Be("Gov Authority of UK");
        org.Details.HasCompanyHouseNumber.Should().Be(true);
        org.Details.CompanyHouseNumber.Should().Be("TestOrg456");
        org.Details.OverseasCompanyNumber.Should().Be("Oversears456");
        org.Details.StartDate.Should().Be(DateTime.Today.AddMonths(10).ToDateTimeOffset());
        org.Details.EndDate.Should().Be(DateTime.Today.AddMonths(5).ToDateTimeOffset());
        org.Details.Addresses.Should().HaveCount(1);

        Address orgAddress = org.Details.Addresses.First();
        orgAddress.StreetAddress.Should().Be("1234 New St");
        orgAddress.Locality.Should().Be("New City");
        orgAddress.Region.Should().Be("New Region");
        orgAddress.PostalCode.Should().Be("SF1 1EX");
        orgAddress.CountryName.Should().Be("New Country");
        orgAddress.Country.Should().Be("NW");
        orgAddress.Type.Should().Be(AddressType.Postal);
    }

    private static void AssertIdentifier(OrganisationInformation.Identifier? identifier)
    {
        identifier.Should().NotBeNull();
        identifier?.LegalName.Should().Be("DefaultLegalName");
    }

    private static void AssertAdditionalParties(IEnumerable<OrganisationReference>? additionalParties)
    {
        additionalParties.Should().NotBeNull();
    }

    private static void AssertAdditionalIdentifiers(IEnumerable<OrganisationInformation.Identifier>? additionalIdentifiers)
    {
        additionalIdentifiers.Should().NotBeNull();
        additionalIdentifiers.Should().HaveCount(1);

        var identifier = additionalIdentifiers?.First();
        identifier?.Scheme.Should().Be("GB-COH");
        identifier?.LegalName.Should().Be("AnotherLegalName");
        identifier?.Uri.Should().Be(new Uri("http://example.com"));
    }

    private static void AssertContactPoint(ContactPoint? contactPoint)
    {
        contactPoint.Should().NotBeNull();
        contactPoint!.Name.Should().Be("Default Contact");
        contactPoint.Email.Should().Be("contact@default.org");
        contactPoint.Telephone.Should().Be("123-456-7890");
        contactPoint.Url.Should().Be("https://contact.default.org");
    }

    private static void AssertRoles(IEnumerable<PartyRole>? roles)
    {
        roles.Should().NotBeNull();
        roles.Should().HaveCount(1);

        var roleStrings = roles!.Select(r => r.AsCode()).ToList();
        roleStrings.Should().Contain("buyer");
    }

    private static void AssertDetails(Details? details)
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
        supplierInformationData?.AnswerSets.First().Answers.Should().HaveCount(7);

        supplierInformationData?.AnswerSets.First().SectionName.Should().NotBeNull();
        supplierInformationData?.AnswerSets.First().SectionName.Should().Be("Localized string");

        supplierInformationData?.AnswerSets.First().Answers.First().QuestionName.Should().NotBeNull();

        supplierInformationData?.Questions.Should().NotBeNull();
        supplierInformationData?.Questions.First().SectionName.Should().Be("Localized string");
        supplierInformationData?.Questions.First().Title.Should().Be("Exclusion_Applies_To");
        supplierInformationData?.Questions.First().Text.Should().Be("Exclusion applies to");

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

        if (answer?.JsonValue != null)
        {
            var json = answer.JsonValue;
            if (json.TryGetValue("id", out object? id))
            {
                var idValue = id.ToString();
                var type = json["type"]?.ToString();

                switch (idValue)
                {
                    case "1db6029d-a077-475b-a6b8-80a79c824787": type.Should().Be("self"); break;
                    case "8f127354-9777-44d3-93dd-a7437e0cc552": type.Should().Be("associated-persons"); break;
                    case "d3d35f4b-953a-4620-8771-fd245d55dd92": type.Should().Be("associated-persons"); break;
                    case "57b1895f-11bb-4cd4-ae38-82f38a70237b": type.Should().Be("additional-entities"); break;
                }
            }
        }
    }

    private void AssertQuestionDetails(FormQuestion? question)
    {
        question.Should().NotBeNull();
        question!.Title.Should().NotBeNullOrWhiteSpace();
        question?.Name.Should().NotBeNullOrWhiteSpace();

        question?.Type.Should().NotBe(FormQuestionType.None);
    }

}