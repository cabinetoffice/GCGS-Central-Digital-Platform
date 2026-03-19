using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationSync;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdatePersonToOrganisationUseCaseTest : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock = new();
    private readonly Mock<IAtomicScope> _atomicScopeMock = new();
    private readonly Mock<IOrganisationMembershipSync> _membershipSyncMock = new();
    private readonly Mock<IFeatureManager> _featureManagerMock = new();
    private readonly UpdatePersonToOrganisationUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();

    public UpdatePersonToOrganisationUseCaseTest(AutoMapperFixture mapperFixture)
    {
        _atomicScopeMock
            .Setup(s => s.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<bool>>, CancellationToken>((action, ct) => action(ct));

        _useCase = new UpdatePersonToOrganisationUseCase(
            _organisationRepositoryMock.Object,
            _atomicScopeMock.Object,
            _membershipSyncMock.Object,
            _featureManagerMock.Object,
            NullLogger<UpdatePersonToOrganisationUseCase>.Instance);
    }

    [Fact]
    public async Task Execute_ShouldUpdatePerson_When_Organisation_Person_Exists()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = ["ADMIN", "EDITOR"] };
        var orgPerson = organisationPerson;
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId)).ReturnsAsync(orgPerson);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);

        var result = await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        result.Should().Be(true);
        _organisationRepositoryMock.Verify(repo => repo.TrackOrganisationPerson(orgPerson!), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldSyncToUm_WhenFeatureFlagEnabled()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = ["ADMIN"] };
        var orgPerson = organisationPerson;
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId)).ReturnsAsync(orgPerson);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);
        _membershipSyncMock
            .Setup(s => s.UpdateMembershipScopesAsync(It.IsAny<UpdateMembershipScopesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SyncError, Unit>.Success(Unit.Value));

        var result = await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        result.Should().Be(true);
        _membershipSyncMock.Verify(s => s.UpdateMembershipScopesAsync(
            It.Is<UpdateMembershipScopesCommand>(c => c.OrganisationGuid == _organisationId && c.PersonGuid == _personId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldNotSyncToUm_WhenFeatureFlagDisabled()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = ["ADMIN"] };
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId)).ReturnsAsync(organisationPerson);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);

        await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        _membershipSyncMock.Verify(s => s.UpdateMembershipScopesAsync(It.IsAny<UpdateMembershipScopesCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldSucceed_WhenSyncFails()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = ["ADMIN"] };
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId)).ReturnsAsync(organisationPerson);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);
        _membershipSyncMock
            .Setup(s => s.UpdateMembershipScopesAsync(It.IsAny<UpdateMembershipScopesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SyncError, Unit>.Failure(new SyncFailureError("UM unavailable")));

        var result = await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        result.Should().Be(true);
    }

    [Fact]
    public async Task Execute_ShouldThrowEmptyPersonRoleException_WhenPersonScopeIsEmpty()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = [] };
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId)).ReturnsAsync(organisationPerson);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        await act.Should().ThrowAsync<EmptyPersonRoleException>()
            .WithMessage($"Empty Scope of Person {_personId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownPersonException_WhenPersonOrOrganisationNotFound()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = ["Viewer"] };
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId)).ReturnsAsync((Persistence.OrganisationPerson?)null);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId} or Person {_personId}.");
    }

    private Persistence.Organisation Organisation =>
        new()
        {
            Guid = _organisationId,
            Name = "Test",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.SupplierInformation()
        };

    private Persistence.Person person =>
        new()
        {
            Guid = _personId,
            FirstName = "Test",
            LastName = "Test",
            Email = "Test@test.com",
            UserUrn = "urn:1234",
        };

    private Persistence.OrganisationPerson organisationPerson =>
        new()
        {
            Organisation = Organisation,
            OrganisationId = Organisation.Id,
            PersonId = person.Id,
            Person = person,
            Scopes = ["Viewer"]
        };
}