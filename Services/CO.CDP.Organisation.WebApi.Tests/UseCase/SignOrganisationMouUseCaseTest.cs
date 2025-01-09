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

public class SignOrganisationMouUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IPersonRepository> _personRepository = new();
    private SignOrganisationMouUseCase _useCase => new SignOrganisationMouUseCase(_organisationRepository.Object, _personRepository.Object);

    [Fact]
    public async Task Execute_ShouldReturnTrue_WhenValidInputs()
    {
        var organisation = FakeOrganisation(true);

        var mou = new Mou { Id = 1, Guid = Guid.NewGuid(), FilePath = "" };

        var person = FakePerson();
        organisation.Persons.Add(person);

        var signMouRequest = new SignMouRequest
        {           
            MouId = mou.Guid,
            Name = "John Doe",
            JobTitle = "CEO",
            CreatedById = person.Guid
        };

        _organisationRepository
            .Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _personRepository
            .Setup(repo => repo.Find(person.Guid))
            .ReturnsAsync(person);

        _organisationRepository
            .Setup(repo => repo.GetMou(mou.Guid))
            .ReturnsAsync(mou);

        _organisationRepository
            .Setup(repo => repo.SaveOrganisationMou(It.IsAny<Persistence.MouSignature>()))
            .Verifiable();

        var result = await _useCase.Execute((organisation.Guid, signMouRequest));

        result.Should().BeTrue();
        _organisationRepository.Verify(repo => repo.SaveOrganisationMou(It.IsAny<Persistence.MouSignature>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        var signMouRequest = new SignMouRequest
        {          
            MouId = Guid.NewGuid(),
            Name = "John Doe",
            JobTitle = "CEO",
            CreatedById = Guid.NewGuid()
        };
        _organisationRepository.Setup(repo => repo.Find(organisationId))
                       .ReturnsAsync((Persistence.Organisation)null!);

        var act = async () => await _useCase.Execute((organisationId, signMouRequest));

        await act.Should().ThrowAsync<UnknownOrganisationException>()
                 .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownPersonException_WhenPersonNotFound()
    {
        var organisation = FakeOrganisation(true);

        var mou = new Mou { Id = 1, Guid = Guid.NewGuid(), FilePath = "" };

        var person = FakePerson();
        organisation.Persons.Add(person);

        var signMouRequest = new SignMouRequest
        {        
            MouId = mou.Guid,
            Name = "John Doe",
            JobTitle = "CEO",
            CreatedById = person.Guid
        };

        _organisationRepository
            .Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _personRepository
            .Setup(repo => repo.Find(person.Guid))
            .ReturnsAsync((Person)null!);

        Func<Task> act = async () => await _useCase.Execute((organisation.Guid, signMouRequest));

        await act.Should().ThrowAsync<UnknownPersonException>()
            .WithMessage($"Unknown person {person.Guid}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownMouException_WhenMouNotFound()
    {
        var organisation = FakeOrganisation(true);

        var mou = new Mou { Id = 1, Guid = Guid.NewGuid(), FilePath = "" };

        var person = FakePerson();
        organisation.Persons.Add(person);

        var signMouRequest = new SignMouRequest
        {           
            MouId = mou.Guid,
            Name = "John Doe",
            JobTitle = "CEO",
            CreatedById = person.Guid
        };
        _organisationRepository
             .Setup(repo => repo.Find(organisation.Guid))
             .ReturnsAsync(organisation);

        _personRepository
            .Setup(repo => repo.Find(person.Guid))
            .ReturnsAsync(person);

        _organisationRepository
            .Setup(repo => repo.GetMou(mou.Guid))
            .ReturnsAsync((Mou)null!);

        Func<Task> act = async () => await _useCase.Execute((organisation.Guid, signMouRequest));

        await act.Should().ThrowAsync<UnknownMouException>()
            .WithMessage($"Unknown Mou {mou.Guid}.");
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