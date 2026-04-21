using CO.CDP.Functional;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Events.Handlers;

public class PersonRemovedHandlerTests
{
    private readonly Mock<IUmOrganisationSyncRepository> _syncRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public PersonRemovedHandlerTests()
    {
        _syncRepo.Setup(r =>
                r.EnsureMemberRemovedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, Unit>.Success(Unit.Value));
    }

    private PersonRemovedHandler CreateSut() =>
        new(_syncRepo.Object, _unitOfWork.Object, NullLogger<PersonRemovedHandler>.Instance);

    private static PersonRemovedFromOrganisation MakeEvent(Guid orgGuid, Guid personGuid) =>
        new() { OrganisationId = orgGuid.ToString(), PersonId = personGuid.ToString() };

    [Fact]
    public async Task Handle_RemovesMember_AndSavesChanges()
    {
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();

        await CreateSut().Handle(MakeEvent(orgGuid, personGuid));

        _syncRepo.Verify(r => r.EnsureMemberRemovedAsync(orgGuid, personGuid, default), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsSyncStepFailedException_WhenRemovalFails()
    {
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        _syncRepo.Setup(r => r.EnsureMemberRemovedAsync(orgGuid, personGuid, default))
            .ReturnsAsync(Result<string, Unit>.Failure("error"));

        var act = () => CreateSut().Handle(MakeEvent(orgGuid, personGuid));

        await act.Should().ThrowAsync<SyncStepFailedException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IsIdempotent_SecondCallSucceeds()
    {
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var @event = MakeEvent(orgGuid, personGuid);
        var sut = CreateSut();

        await sut.Handle(@event);
        await sut.Handle(@event);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Exactly(2));
    }
}