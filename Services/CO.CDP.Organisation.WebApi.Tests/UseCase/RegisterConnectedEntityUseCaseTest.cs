using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RegisterConnectedEntityUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<Persistence.IOrganisationRepository> _repository = new();
    private readonly Mock<Persistence.IPersonRepository> _persons = new();
    private readonly Mock<Persistence.IConnectedEntityRepository> _connectedEntityRepo = new();
    private readonly Guid _generatedGuid = Guid.NewGuid();
    private RegisterConnectedEntityUseCase UseCase => new(_connectedEntityRepo.Object, _repository.Object, mapperFixture.Mapper, () => _generatedGuid);

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        Persistence.Organisation? organisation = null;
        var registerConnectedEntity = GivenRegisterConnectedEntity(organisationId);

        _repository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        Func<Task> act = async () => await UseCase.Execute((organisationId, registerConnectedEntity));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    private RegisterConnectedEntity GivenRegisterConnectedEntity(Guid organisationId)
    {
        return new RegisterConnectedEntity
        {
            EntityType = ConnectedEntityType.Organisation,
            Organisation = new ConnectedOrganisation
            {
                Category = ConnectedOrganisationCategory.RegisteredCompany,
                Name = "Org1",
                OrganisationId = organisationId
            },
            Addresses = [ new Address
                {
                    Type = AddressType.Registered,
                    StreetAddress = "1234 Example St",
                    StreetAddress2 = "",
                    Locality = "Example City",
                    Region = "Test Region",
                    PostalCode = "12345",
                    CountryName = "Exampleland"
                }],
            RegisteredDate = DateTime.Now,
            CompanyHouseNumber = "CH_1"
        };
    }

    private Persistence.Person GivenPersonExists(Guid guid)
    {
        Persistence.Person person = new Persistence.Person
        {
            Id = 13,
            Guid = guid,
            FirstName = "Bob",
            LastName = "Smith",
            Email = "contact@example.com"
        };
        _persons.Setup(r => r.Find(guid)).ReturnsAsync(person);
        return person;
    }
}