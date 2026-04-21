using CO.CDP.Functional;
using CO.CDP.UserManagement.Api.Events;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Events.Handlers;

public class OrganisationRegisteredHandlerTests
{
    private readonly Mock<IUmOrganisationSyncRepository> _syncRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public OrganisationRegisteredHandlerTests()
    {
        _syncRepo.Setup(r => r.EnsureCreatedAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, Unit>.Success(Unit.Value));
        _syncRepo.Setup(r => r.EnsureActiveApplicationsEnabledAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, Unit>.Success(Unit.Value));
        _syncRepo.Setup(r => r.EnsureFounderOwnerCreatedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, Unit>.Success(Unit.Value));
    }

    private OrganisationRegisteredHandler CreateSut() =>
        new(_syncRepo.Object, _unitOfWork.Object, NullLogger<OrganisationRegisteredHandler>.Instance);

    private static OrganisationRegistered MakeEvent(Guid orgGuid, string name,
        Guid? founderPersonId = null, string? founderUrn = null) =>
        new()
        {
            Id = orgGuid.ToString(),
            Name = name,
            Roles = ["supplier"],
            Type = 2,
            FounderPersonId = founderPersonId,
            FounderUserUrn = founderUrn
        };

    [Fact]
    public async Task Handle_CreatesOrg_EnablesApps_CreatesFounder_WhenAllInfoPresent()
    {
        var orgGuid = Guid.NewGuid();
        var founderGuid = Guid.NewGuid();

        await CreateSut().Handle(MakeEvent(orgGuid, "Acme", founderGuid, "urn:user:1"));

        _syncRepo.Verify(r => r.EnsureCreatedAsync(
            orgGuid, "Acme", default), Times.Once);
        _syncRepo.Verify(r => r.EnsureActiveApplicationsEnabledAsync(orgGuid, default), Times.Once);
        _syncRepo.Verify(r => r.EnsureFounderOwnerCreatedAsync(
            orgGuid, founderGuid, "urn:user:1", default), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_SkipsFounderCreation_WhenNoFounderInfo()
    {
        var orgGuid = Guid.NewGuid();

        await CreateSut().Handle(MakeEvent(orgGuid, "Acme"));

        _syncRepo.Verify(r => r.EnsureFounderOwnerCreatedAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_ThrowsSyncStepFailedException_WhenCreateFails()
    {
        var orgGuid = Guid.NewGuid();
        _syncRepo.Setup(r => r.EnsureCreatedAsync(
                orgGuid, "Acme", default))
            .ReturnsAsync(Result<string, Unit>.Failure("DB error"));

        var act = () => CreateSut().Handle(MakeEvent(orgGuid, "Acme"));

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

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Exactly(6));
    }
}