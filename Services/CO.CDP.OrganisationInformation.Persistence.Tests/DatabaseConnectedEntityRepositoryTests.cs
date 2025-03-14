using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.OrganisationInformation.Persistence.Tests.Factories;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using System;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseConnectedEntityRepositoryTests(PostgreSqlFixture postgreSql)
    : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItReturnsNullIfConnectedEntityIsNotFound()
    {
        using var repository = ConnectedEntityRepository();

        var found = await repository.Find(Guid.NewGuid(), Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public async Task IsConnectedEntityUsedInExclusionAsync_ShouldReturnTrue_WhenConnectedPersonIsUsedInExclusion()
    {
        using var repositoryCE = ConnectedEntityRepository();
        await using var context = GetDbContext();
        var orgId = Guid.NewGuid();
        var connectedEntityId = Guid.Parse("fcbc270c-7da9-4d04-8d97-b89a3af7e381");
        var person = GivenPerson();
        var organisation = GivenOrganisation(guid: orgId, personsWithScope: [(person, ["ADMIN"])]);
        var form = SharedConsentFactory.GivenForm();
        var section = SharedConsentFactory.GivenFormSection(form: form);
        section.Type = FormSectionType.Exclusions;
        var sharedConsent = SharedConsentFactory.GivenSharedConsent(
            state: SubmissionState.Submitted,
            organisation: organisation,
            form: form);
        var answerSetId = Guid.NewGuid();
        var answerSet = SharedConsentFactory.GivenAnswerSet(sharedConsent: sharedConsent, answerSetId: answerSetId, section: section);
        var question = SharedConsentFactory.GivenFormQuestion(type: FormQuestionType.Text, section: section);
        var formAnswer = new FormAnswer
        {
            Guid = Guid.NewGuid(),
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            JsonValue = "{\n  \"id\": \"fcbc270c-7da9-4d04-8d97-b89a3af7e381\",\n  \"type\": \"connected-entity\"\n}",
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        answerSet.Answers.Add(formAnswer);
        context.FormAnswerSets.Add(answerSet);
        context.Organisations.Add(organisation);
        context.SharedConsents.Add(sharedConsent);
        await context.SaveChangesAsync();

        var expectedEntity = new ConnectedEntity
        {
            Guid = connectedEntityId,
            EntityType = ConnectedEntity.ConnectedEntityType.Organisation,
            Organisation = new ConnectedEntity.ConnectedOrganisation
            {
                OrganisationId = orgId,
                Name = "CHN_111",
                Category = ConnectedEntity.ConnectedOrganisationCategory.DirectorOrTheSameResponsibilities,
                RegisteredLegalForm = "Legal Form",
                LawRegistered = "Law Registered"
            },
            SupplierOrganisation = organisation
        };

        context.ConnectedEntities.Add(expectedEntity);
        await context.SaveChangesAsync();

        var result = await repositoryCE.IsConnectedEntityUsedInExclusionAsync(organisation.Guid, connectedEntityId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsConnectedEntityUsedInExclusionAsync_ShouldReturnFalse_WhenConnectedPersonNotUsedInExclusion()
    {
        using var repositoryCE = ConnectedEntityRepository();
        using var repositoryOrg = OrganisationRepository();
        var orgId = Guid.NewGuid();
        var connectedEntityId = Guid.NewGuid();
        var person = GivenPerson();
        var organisation = GivenOrganisation(guid: orgId, personsWithScope: [(person, ["ADMIN"])]);

        repositoryOrg.Save(organisation);

        var expectedEntity = new ConnectedEntity
        {
            Guid = connectedEntityId,
            EntityType = ConnectedEntity.ConnectedEntityType.Organisation,
            Organisation = new ConnectedEntity.ConnectedOrganisation
            {
                OrganisationId = orgId,
                Name = "CHN_111",
                Category = ConnectedEntity.ConnectedOrganisationCategory.DirectorOrTheSameResponsibilities,
                RegisteredLegalForm = "Legal Form",
                LawRegistered = "Law Registered"
            },
            SupplierOrganisation = organisation
        };

        await repositoryCE.Save(expectedEntity);
        var result = await repositoryCE.IsConnectedEntityUsedInExclusionAsync(organisation.Guid, connectedEntityId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Find_ShouldReturnConnectedEntity_WhenEntityExists()
    {
        using var repositoryCE = ConnectedEntityRepository();
        using var repositoryOrg = OrganisationRepository();
        var guid = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var person = GivenPerson();
        var organisation = GivenOrganisation(guid: orgId, personsWithScope: [(person, ["ADMIN"])]);

        repositoryOrg.Save(organisation);

        var expectedEntity = new ConnectedEntity
        {
            Guid = guid,
            EntityType = ConnectedEntity.ConnectedEntityType.Organisation,
            Organisation = new ConnectedEntity.ConnectedOrganisation
            {
                OrganisationId = orgId,
                Name = "CHN_111",
                Category = ConnectedEntity.ConnectedOrganisationCategory.DirectorOrTheSameResponsibilities,
                RegisteredLegalForm = "Legal Form",
                LawRegistered = "Law Registered"
            },
            SupplierOrganisation = organisation
        };

        await repositoryCE.Save(expectedEntity);

        var result = await repositoryCE.Find(orgId, guid);

        result.Should().BeEquivalentTo(expectedEntity);
    }

    [Fact]
    public async Task GetSummary_ShouldReturnConnectedEntityLookups_WhenEntitiesExist()
    {
        var organisationId = Guid.NewGuid();
        var guid = Guid.NewGuid();
        using var repositoryCE = ConnectedEntityRepository();
        using var repositoryOrg = OrganisationRepository();
        var person = GivenPerson();
        var organisation = GivenOrganisation(guid: organisationId, personsWithScope: [(person, ["ADMIN"])]);

        repositoryOrg.Save(organisation);
        var ce1 = new ConnectedEntity
        {
            Guid = guid,
            EntityType = ConnectedEntity.ConnectedEntityType.Organisation,
            Organisation = new ConnectedEntity.ConnectedOrganisation
            {
                OrganisationId = organisationId,
                Name = "CHN_111",
                Category = ConnectedEntity.ConnectedOrganisationCategory.DirectorOrTheSameResponsibilities,
                RegisteredLegalForm = "Legal Form",
                LawRegistered = "Law Registered"
            },
            SupplierOrganisation = organisation
        };
        var ce2 = new ConnectedEntity
        {
            Guid = Guid.NewGuid(),
            EntityType = ConnectedEntity.ConnectedEntityType.TrustOrTrustee,
            IndividualOrTrust = new ConnectedEntity.ConnectedIndividualTrust
            {
                FirstName = "First Name",
                LastName = "Last Name",
                Category = ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForIndiv,
                ConnectedType = ConnectedEntity.ConnectedPersonType.Individual
            },
            SupplierOrganisation = organisation
        };

        var connectedEntities = new List<ConnectedEntity>() { ce1, ce2 };

        await repositoryCE.Save(ce1);
        await repositoryCE.Save(ce2);

        var result = await repositoryCE.GetSummary(organisationId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(x => x!.Name == "CHN_111");
        result.Should().ContainSingle(x => x!.Name == "First Name Last Name");
    }

    private DatabaseConnectedEntityRepository ConnectedEntityRepository()
        => new(GetDbContext());

    private DatabaseOrganisationRepository OrganisationRepository()
        => new(GetDbContext());

    private OrganisationInformationContext? context = null;

    private OrganisationInformationContext GetDbContext()
    {
        context = context ?? postgreSql.OrganisationInformationContext();
        return context;
    }
}