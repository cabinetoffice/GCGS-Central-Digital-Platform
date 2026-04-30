using CO.CDP.Functional;
using CO.CDP.UserManagement.Api.Events;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Events.Handlers;

public class OrganisationUpdatedHandlerTests
{
    private readonly Mock<IUmOrganisationSyncRepository> _syncRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public OrganisationUpdatedHandlerTests()
    {
        _syncRepo.Setup(r =>
                r.EnsureNameSyncedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, Unit>.Success(Unit.Value));
    }

    private OrganisationUpdatedHandler CreateSut() =>
        new(_syncRepo.Object, _unitOfWork.Object, NullLogger<OrganisationUpdatedHandler>.Instance);

    private static OrganisationUpdated MakeEvent(Guid orgGuid, string name) =>
        new() { Id = orgGuid.ToString(), Name = name };

    [Fact]
    public async Task Handle_SyncsName_AndSavesChanges()
    {
        var orgGuid = Guid.NewGuid();

        await CreateSut().Handle(MakeEvent(orgGuid, "New Name"));

        _syncRepo.Verify(r => r.EnsureNameSyncedAsync(orgGuid, "New Name", default), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsSyncStepFailedException_WhenNameSyncFails()
    {
        var orgGuid = Guid.NewGuid();
        _syncRepo.Setup(r => r.EnsureNameSyncedAsync(orgGuid, "Fail Corp", default))
            .ReturnsAsync(Result<string, Unit>.Failure("not found"));

        var act = () => CreateSut().Handle(MakeEvent(orgGuid, "Fail Corp"));

        await act.Should().ThrowAsync<SyncStepFailedException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IsIdempotent_SecondCallSucceeds()
    {
        var orgGuid = Guid.NewGuid();
        var @event = MakeEvent(orgGuid, "Acme");
        var sut = CreateSut();

        await sut.Handle(@event);
        await sut.Handle(@event);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Exactly(2));
    }
}