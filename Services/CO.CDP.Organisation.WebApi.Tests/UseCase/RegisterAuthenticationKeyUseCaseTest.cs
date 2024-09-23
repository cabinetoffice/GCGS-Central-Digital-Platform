using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RegisterAuthenticationKeyUseCaseTest()
{
    private readonly Mock<IOrganisationRepository> _organisationRepo = new();
    private readonly Mock<IAuthenticationKeyRepository> _keyRepo = new();
    private RegisterAuthenticationKeyUseCase UseCase => new(_keyRepo.Object, _organisationRepo.Object);

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        Persistence.Organisation? organisation = null;
        var registerAuthenticationKey = GivenRegisterAuthenticationKey(organisationId);

        _organisationRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        Func<Task> act = async () => await UseCase.Execute((organisationId, registerAuthenticationKey));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task ItReturnsTheRegisteredAuthenticationKey()
    {
        var organisationId = Guid.NewGuid();

        var command = (organisationId, GivenRegisterAuthenticationKey(organisationId));

        var org = GivenOrganisationExist(organisationId);

        _organisationRepo.Setup(x => x.Save(org));

        var result = await UseCase.Execute(command);

        var expectedAuthenticationKey = GivenRegisterAuthenticationKey(organisationId);

        _keyRepo.Verify(repo => repo.Save(It.IsAny<Persistence.AuthenticationKey>()), Times.Once);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ItSavesNewAuthenticationKeyInTheRepository()
    {
        var organisationId = Guid.NewGuid();
        var registerAuthenticationKey = GivenRegisterAuthenticationKey(organisationId);
        var org = GivenOrganisationExist(organisationId);

        var command = (organisationId, registerAuthenticationKey);

        Persistence.AuthenticationKey? persistanceAuthenticationKey = null;

        _organisationRepo.Setup(x => x.Save(org));

        _keyRepo
            .Setup(x => x.Save(It.IsAny<Persistence.AuthenticationKey>()))
            .Callback<Persistence.AuthenticationKey>(b => persistanceAuthenticationKey = b);

        var result = await UseCase.Execute(command);

        _keyRepo.Verify(e => e.Save(It.IsAny<Persistence.AuthenticationKey>()), Times.Once);

        persistanceAuthenticationKey.Should().NotBeNull();
        persistanceAuthenticationKey.As<Persistence.AuthenticationKey>().OrganisationId.Should().Be(1);
        persistanceAuthenticationKey.As<Persistence.AuthenticationKey>().Key.Should().Be("Key1");
    }

    private Persistence.Organisation GivenOrganisationExist(Guid organisationId)
    {
        var org = new Persistence.Organisation
        {
            Id = 1,
            Name = "TheOrganisation",
            Guid = organisationId,
            Addresses = [new Persistence.Organisation.OrganisationAddress
            {
                Type = AddressType.Registered,
                Address = new Persistence.Address
                {
                    StreetAddress = "1234 Example St",
                    Locality = "Example City",
                    Region = "Test Region",
                    PostalCode = "12345",
                    CountryName = "Exampleland",
                    Country = "AB"
                }
            }],
            Tenant = It.IsAny<Tenant>(),
            SupplierInfo = new Persistence.Organisation.SupplierInformation()
        };

        _organisationRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(org);

        return org;
    }

    private static RegisterAuthenticationKey GivenRegisterAuthenticationKey(Guid organisationId)
    {
        return new RegisterAuthenticationKey
        {
            Key = "Key1",
            Name = "KeyName1",
            OrganisationId = organisationId
        };
    }
}