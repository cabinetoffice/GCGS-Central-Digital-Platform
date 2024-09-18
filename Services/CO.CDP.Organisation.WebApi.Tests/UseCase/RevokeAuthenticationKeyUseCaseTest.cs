using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RevokeAuthenticationKeyUseCaseTest()
{
    private readonly Mock<IOrganisationRepository> _organisationRepo = new();
    private readonly Mock<IAuthenticationKeyRepository> _keyRepo = new();
    private RevokeAuthenticationKeyUseCase UseCase => new(_organisationRepo.Object, _keyRepo.Object);

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        Persistence.Organisation? organisation = null;
        var registerAuthenticationKey = GivenRevokeAuthenticationKey();

        _organisationRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        Func<Task> act = async () => await UseCase.Execute((organisationId, registerAuthenticationKey));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowEmptyAuthenticationKeyException_WhenKeyIsNullOrEmpty()
    {
        var organisationId = Guid.NewGuid();
        var registerAuthenticationKey = GivenRevokeAuthenticationKey();
        registerAuthenticationKey.Key = "";

        var org = GivenOrganisationExist(organisationId);

        _organisationRepo.Setup(x => x.Save(org));

        _organisationRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(org);

        Func<Task> act = async () => await UseCase.Execute((organisationId, registerAuthenticationKey));

        await act.Should()
            .ThrowAsync<EmptyAuthenticationKeyException>()
            .WithMessage($"Empty Key of Revoke AuthenticationKey for organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowEmptyAuthenticationKeyNameException_WhenNameIsNullOrEmpty()
    {
        var organisationId = Guid.NewGuid();
        var registerAuthenticationKey = GivenRevokeAuthenticationKey();
        registerAuthenticationKey.Name = "";

        var org = GivenOrganisationExist(organisationId);

        _organisationRepo.Setup(x => x.Save(org));

        _organisationRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(org);

        Func<Task> act = async () => await UseCase.Execute((organisationId, registerAuthenticationKey));

        await act.Should()
            .ThrowAsync<EmptyAuthenticationKeyNameException>()
            .WithMessage($"Empty Name of Revoke AuthenticationKey for organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownAuthenticationKeyException_WhenKeyNotFound()
    {
        var organisationId = Guid.NewGuid();
        Persistence.AuthenticationKey? authorisationKey = null;
        var registerAuthenticationKey = GivenRevokeAuthenticationKey();

        var org = GivenOrganisationExist(organisationId);

        _organisationRepo.Setup(x => x.Save(org));

        _organisationRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(org);

        _keyRepo.Setup(k => k.FindByKeyNameAndOrganisationId("k", "n", organisationId))
            .ReturnsAsync(authorisationKey);

        Func<Task> act = async () => await UseCase.Execute((organisationId, registerAuthenticationKey));

        await act.Should()
        .ThrowAsync<UnknownAuthenticationKeyException>()
            .WithMessage($"Unknown Authentication Key - name {registerAuthenticationKey.Name} for organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldRevokeAuthenticationKey_WhenValidCommandIsProvided()
    {
        var organisationId = Guid.NewGuid();
        var revokeAuth = GivenRevokeAuthenticationKey();
        var command = (organisationId, revokeAuth);
        var organisation = GivenOrganisationExist(organisationId);
        var authKey = new Persistence.AuthenticationKey { Key = revokeAuth.Key, Name = revokeAuth.Name, Revoked = false };

        _organisationRepo.Setup(repo => repo.Find(organisationId)).ReturnsAsync(organisation);
        _keyRepo.Setup(repo => repo.FindByKeyNameAndOrganisationId(revokeAuth.Key, revokeAuth.Name, organisationId)).ReturnsAsync(authKey);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        authKey.Revoked.Should().BeTrue();
        _keyRepo.Verify(repo => repo.Save(authKey), Times.Once);
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

    private static RevokeAuthenticationKey GivenRevokeAuthenticationKey()
    {
        return new RevokeAuthenticationKey
        {
            Key = "Key1",
            Name = "KeyName1"
        };
    }
}