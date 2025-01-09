using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Mou = CO.CDP.OrganisationInformation.Persistence.Mou;
using MouSignature = CO.CDP.OrganisationInformation.Persistence.MouSignature;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetOrganisationMouSignaturesUseCaseTest(AutoMapperFixture mapperFixture)
    : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _repository = new();
    private GetOrganisationMouSignaturesUseCase _useCase => new GetOrganisationMouSignaturesUseCase(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_ShouldReturnMappedMouSignatures_WhenOrganisationExists()
    {
        var organisation = FakeOrganisation(true);

        var mou = new Mou { Id = 1, Guid = Guid.NewGuid(), FilePath = "" };

        var person = FakePerson();
        organisation.Persons.Add(person);

        var mouSignatures = new List<Persistence.MouSignature>
        {
            new Persistence.MouSignature { Id = 1, SignatureGuid = Guid.NewGuid(),Name="Jo Bloggs", JobTitle = "Manager", OrganisationId = organisation.Id, Organisation=organisation, CreatedById = person.Id, CreatedBy=person, MouId = mou.Id, Mou=mou, CreatedOn = DateTimeOffset.UtcNow, UpdatedOn = DateTimeOffset.UtcNow },
            new Persistence.MouSignature { Id = 2, SignatureGuid = Guid.NewGuid(), Name="Steve V", JobTitle = "Director", OrganisationId = organisation.Id, Organisation=organisation, CreatedById= person.Id, CreatedBy=person, MouId = mou.Id, Mou=mou, CreatedOn = DateTimeOffset.UtcNow, UpdatedOn = DateTimeOffset.UtcNow },
        };

        _repository.Setup(repo => repo.Find(organisation.Guid))
               .ReturnsAsync(organisation);
        _repository.Setup(repo => repo.GetMouSignatures(organisation.Id))
                       .ReturnsAsync(mouSignatures);

        var result = await _useCase.Execute(organisation.Guid);

        result.Should().SatisfyRespectively(
            first =>
            {
                first.Id.Should().Be(mouSignatures[0].SignatureGuid);
                first.JobTitle.Should().Be("Manager");
                first.SignatureOn.Should().BeCloseTo(mouSignatures[0].CreatedOn, TimeSpan.FromSeconds(1));
            },
            second =>
            {
                second.Id.Should().Be(mouSignatures[1].SignatureGuid);
                second.JobTitle.Should().Be("Director");
                second.SignatureOn.Should().BeCloseTo(mouSignatures[1].CreatedOn, TimeSpan.FromSeconds(1));
            }
        );
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        _repository.Setup(repo => repo.Find(organisationId))
                       .ReturnsAsync((Persistence.Organisation)null!);

        var act = async () => await _useCase.Execute(organisationId);

        await act.Should().ThrowAsync<UnknownOrganisationException>()
                 .WithMessage($"Unknown organisation {organisationId}.");
    }

    public static Person FakePerson(
       Guid? guid = null,
       string? userUrn = null,
       string firstname = "Jon",
       string lastname = "doe",
       string? email = null,
       string phone = "07925123123",
       List<string>? scopes = null,
       Tenant? tenant = null,
       List<(Persistence.Organisation, List<string>)>? organisationsWithScope = null
   )
    {
        scopes = scopes ?? [];
        var personGuid = guid ?? Guid.NewGuid();
        var person = new Person
        {
            Guid = personGuid,
            UserUrn = userUrn ?? $"urn:fdc:gov.uk:2022:{Guid.NewGuid()}",
            FirstName = firstname,
            LastName = lastname,
            Email = email ?? $"jon{personGuid}@example.com",
            Phone = phone,
            Scopes = scopes
        };
        if (tenant != null)
        {
            person.Tenants.Add(tenant);
        }

        foreach (var organisationWithScope in organisationsWithScope ?? [])
        {
            person.PersonOrganisations.Add(
                new OrganisationPerson
                {
                    Person = person,
                    Organisation = organisationWithScope.Item1,
                    Scopes = organisationWithScope.Item2
                }
            );
        }

        return person;
    }

    private static Persistence.Organisation FakeOrganisation(bool? withBuyerInfo = true)
    {
        Persistence.Organisation org = new()
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "FakeOrg",
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
            },
            ContactPoints =
            [
                new Persistence.Organisation.ContactPoint
                {
                    Email = "contact@test.org"
                }
            ],
            Type = OrganisationType.Organisation
        };

        if (withBuyerInfo == true)
        {
            var devolvedRegulations = new List<DevolvedRegulation>();
            devolvedRegulations.Add(DevolvedRegulation.NorthernIreland);

            org.BuyerInfo = new Persistence.Organisation.BuyerInformation
            {
                BuyerType = "FakeBuyerType",
                DevolvedRegulations = devolvedRegulations,
            };
        }

        return org;
    }
}