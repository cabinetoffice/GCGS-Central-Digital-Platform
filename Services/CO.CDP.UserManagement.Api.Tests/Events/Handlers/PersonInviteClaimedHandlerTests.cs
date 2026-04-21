using CO.CDP.Functional;
using CO.CDP.UserManagement.Api.Events;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Events.Handlers;

public class PersonInviteClaimedHandlerTests
{
    private readonly Mock<IUmOrganisationSyncRepository> _syncRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public PersonInviteClaimedHandlerTests()
    {
        _syncRepo.Setup(r => r.EnsureMemberCreatedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, Unit>.Success(Unit.Value));
    }

    private PersonInviteClaimedHandler CreateSut() =>
        new(_syncRepo.Object, _unitOfWork.Object, NullLogger<PersonInviteClaimedHandler>.Instance);

    private static PersonInviteClaimed MakeEvent(Guid orgGuid, Guid personGuid,
        string urn = "urn:test:1", List<string>? scopes = null) =>
        new()
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            UserPrincipalId = urn,
            Scopes = scopes ?? ["EDITOR"]
        };

    [Fact]
    public async Task Handle_CreatesMember_WithCorrectScopes()
    {
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();

        await CreateSut().Handle(MakeEvent(orgGuid, personGuid, "urn:test:42", ["EDITOR"]));

        _syncRepo.Verify(r => r.EnsureMemberCreatedAsync(
            orgGuid, personGuid, "urn:test:42",
            It.Is<IReadOnlyList<string>>(s => s.Contains("EDITOR")),
            default), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsSyncStepFailedException_WhenCreateFails()
    {
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        _syncRepo.Setup(r => r.EnsureMemberCreatedAsync(
                orgGuid, personGuid, It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), default))
            .ReturnsAsync(Result<string, Unit>.Failure("creation failed"));

        var act = () => CreateSut().Handle(MakeEvent(orgGuid, personGuid));

        await act.Should().ThrowAsync<SyncStepFailedException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}