using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateInvitedPersonToOrganisationUseCaseTests : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly Mock<IPersonInviteRepository> _personInviteRepositoryMock;
    private readonly UpdateInvitedPersonToOrganisationUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _personInviteId = Guid.NewGuid();

    public UpdateInvitedPersonToOrganisationUseCaseTests(AutoMapperFixture mapperFixture)
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _personInviteRepositoryMock = new Mock<IPersonInviteRepository>();
        _useCase = new UpdateInvitedPersonToOrganisationUseCase(_organisationRepositoryMock.Object, _personInviteRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldUpdatePersonInvite_When_Organisation_PersonInvite_Exists()
    {
        var updatePersonToOrganisation = new UpdateInvitedPersonToOrganisation
        {
            Scopes = ["Editor"]
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);
        var personInvite = PersonInvite;
        _personInviteRepositoryMock.Setup(repo => repo.Find(_personInviteId)).ReturnsAsync(personInvite);

        var result = await _useCase.Execute((_organisationId, _personInviteId, updatePersonToOrganisation));

        result.Should().Be(true);
        _personInviteRepositoryMock.Verify(repo => repo.Save(personInvite!), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowEmptyPersonRoleException_WhenPersonInviteScopeIsEmpty()
    {
        var updatePersonToOrganisation = new UpdateInvitedPersonToOrganisation
        {
            Scopes = []
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        _personInviteRepositoryMock.Setup(repo => repo.Find(_personInviteId)).ReturnsAsync(PersonInvite);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _personInviteId, updatePersonToOrganisation));

        await act.Should()
            .ThrowAsync<EmptyPersonRoleException>()
            .WithMessage($"Empty Scope of Invited Person {_personInviteId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownInvitedPersonException_WhenPersonInviteNotFound()
    {
        var updatePersonToOrganisation = new UpdateInvitedPersonToOrganisation
        {
            Scopes = ["Viewer"]
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _personInviteId, updatePersonToOrganisation));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var updatePersonToOrganisation = new UpdateInvitedPersonToOrganisation
        {
            Scopes = ["Viewer"]
        };

        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);
        _personInviteRepositoryMock.Setup(repo => repo.Find(_personInviteId)).ReturnsAsync((Persistence.PersonInvite?)null);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _personInviteId, updatePersonToOrganisation));

        await act.Should()
            .ThrowAsync<UnknownInvitedPersonException>()
            .WithMessage($"Unknown invited person {_personInviteId}.");
    }

    private Persistence.Organisation Organisation =>
        new Persistence.Organisation
        {
            Guid = _organisationId,
            Name = "Test",
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.Organisation.SupplierInformation()
        };

    private Persistence.PersonInvite PersonInvite =>
       new Persistence.PersonInvite
       {
           Guid = _personInviteId,
           FirstName = "Test",
           LastName = "Test",
           Email = "Test@test.com",
           Organisation = Organisation,
           InviteSentOn = DateTime.UtcNow,
           Scopes = ["Viewer"]
       };
}