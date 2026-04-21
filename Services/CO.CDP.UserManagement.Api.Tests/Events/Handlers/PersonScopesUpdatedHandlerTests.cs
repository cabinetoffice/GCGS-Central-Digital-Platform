using CO.CDP.Functional;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Events.Handlers;

public class PersonScopesUpdatedHandlerTests
{
    private readonly Mock<IUmOrganisationSyncRepository> _syncRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public PersonScopesUpdatedHandlerTests()
    {
        _syncRepo.Setup(r => r.EnsureMemberScopesAndAppRolesUpdatedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, Unit>.Success(Unit.Value));
    }

    private PersonScopesUpdatedHandler CreateSut() =>
        new(_syncRepo.Object, _unitOfWork.Object, NullLogger<PersonScopesUpdatedHandler>.Instance);

    private static PersonScopesUpdated MakeEvent(Guid orgGuid, Guid personGuid,
        List<string>? scopes = null) =>
        new()
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            Scopes = scopes ?? ["ADMIN"]
        };

    [Fact]
    public async Task Handle_UpdatesScopesAndAppRoles()
    {
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();

        await CreateSut().Handle(MakeEvent(orgGuid, personGuid, ["ADMIN"]));

        _syncRepo.Verify(r => r.EnsureMemberScopesAndAppRolesUpdatedAsync(
            orgGuid, personGuid,
            It.Is<IReadOnlyList<string>>(s => s.Contains("ADMIN")),
            default), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsSyncStepFailedException_WhenUpdateFails()
    {
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        _syncRepo.Setup(r => r.EnsureMemberScopesAndAppRolesUpdatedAsync(
                orgGuid, personGuid, It.IsAny<IReadOnlyList<string>>(), default))
            .ReturnsAsync(Result<string, Unit>.Failure("update failed"));

        var act = () => CreateSut().Handle(MakeEvent(orgGuid, personGuid));

        await act.Should().ThrowAsync<SyncStepFailedException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}