using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetLatestMouUseCaseTest(AutoMapperFixture mapperFixture)
    : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private GetLatestMouUseCase _useCase => new GetLatestMouUseCase(_organisationRepository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_ShouldReturnMappedMou_WhenLatestMouExists()
    {   
        var latestMouEntity = new Persistence.Mou
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            FilePath = "/path/to/mou.pdf",
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        var mappedMou = new CO.CDP.Organisation.WebApi.Model.Mou
        {
            Id = latestMouEntity.Guid,
            FilePath = latestMouEntity.FilePath,
            CreatedOn = latestMouEntity.CreatedOn
        };

        _organisationRepository
            .Setup(repo => repo.GetLatestMou())
            .ReturnsAsync(latestMouEntity);    

        var result = await _useCase.Execute();
        
        result.Should().BeEquivalentTo(mappedMou);

        _organisationRepository.Verify(repo => repo.GetLatestMou(), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownMouException_WhenLatestMouIsNull()
    {
        _organisationRepository
            .Setup(repo => repo.GetLatestMou())
            .ReturnsAsync((Persistence.Mou)null!);

        Func<Task> act = async () => await _useCase.Execute();

        await act.Should().ThrowAsync<UnknownMouException>()
            .WithMessage("No MOU found.");

        _organisationRepository.Verify(repo => repo.GetLatestMou(), Times.Once);
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