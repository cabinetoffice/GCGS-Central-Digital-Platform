using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.UnitTests.Controllers;

public class OrganisationInvitesControllerTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepository;
    private readonly Mock<IPendingOrganisationInviteRepository> _pendingInviteRepository;
    private readonly Mock<IInviteOrchestrationService> _inviteOrchestrationService;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly OrganisationInvitesController _controller;

    public OrganisationInvitesControllerTests()
    {
        _organisationRepository = new Mock<IOrganisationRepository>();
        _pendingInviteRepository = new Mock<IPendingOrganisationInviteRepository>();
        _inviteOrchestrationService = new Mock<IInviteOrchestrationService>();
        _currentUserService = new Mock<ICurrentUserService>();
        var logger = new Mock<ILogger<OrganisationInvitesController>>();
        _controller = new OrganisationInvitesController(
            _organisationRepository.Object,
            _pendingInviteRepository.Object,
            _inviteOrchestrationService.Object,
            _currentUserService.Object,
            logger.Object);
    }

    [Fact]
    public async Task GetInvites_WhenOrganisationNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        _organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organisation?)null);

        var result = await _controller.GetInvites(orgId, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<PendingOrganisationInviteResponse>>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetInvites_WhenOrganisationFound_ReturnsOk()
    {
        var orgId = Guid.NewGuid();
        var organisation = new Organisation { Id = 12, CdpOrganisationGuid = orgId, Name = "Org", Slug = "org" };
        var invites = new List<PendingOrganisationInvite>
        {
            new()
            {
                Id = 5,
                OrganisationId = organisation.Id,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                OrganisationRole = OrganisationRole.Admin,
                CdpPersonInviteGuid = Guid.NewGuid(),
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }
        };
        _organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _pendingInviteRepository.Setup(repo => repo.GetByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invites);

        var result = await _controller.GetInvites(orgId, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<PendingOrganisationInviteResponse>>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<PendingOrganisationInviteResponse>>().Subject.ToList();
        response.Should().HaveCount(1);
        response[0].PendingInviteId.Should().Be(5);
    }

    [Fact]
    public async Task InviteUser_WhenValid_ReturnsCreated()
    {
        var orgId = Guid.NewGuid();
        var request = new InviteUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            OrganisationRole = OrganisationRole.Admin
        };
        var invite = new PendingOrganisationInvite
        {
            Id = 9,
            OrganisationId = 12,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            OrganisationRole = request.OrganisationRole,
            CdpPersonInviteGuid = Guid.NewGuid(),
            CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _currentUserService.Setup(service => service.GetUserPrincipalId()).Returns("inviter");
        _inviteOrchestrationService.Setup(service => service.InviteUserAsync(
                orgId,
                request,
                "inviter",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        var result = await _controller.InviteUser(orgId, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PendingOrganisationInviteResponse>>().Subject;
        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(OrganisationInvitesController.GetInvites));
        created.RouteValues.Should().ContainKey("cdpOrganisationId").WhoseValue.Should().Be(orgId);
        var response = created.Value.Should().BeOfType<PendingOrganisationInviteResponse>().Subject;
        response.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task InviteUser_WhenDuplicate_ReturnsConflict()
    {
        var orgId = Guid.NewGuid();
        var request = new InviteUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            OrganisationRole = OrganisationRole.Admin
        };
        _inviteOrchestrationService.Setup(service => service.InviteUserAsync(
                orgId,
                request,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEntityException("PendingOrganisationInvite", "Email", request.Email));

        var result = await _controller.InviteUser(orgId, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PendingOrganisationInviteResponse>>().Subject;
        actionResult.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task InviteUser_WhenNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        var request = new InviteUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            OrganisationRole = OrganisationRole.Admin
        };
        _inviteOrchestrationService.Setup(service => service.InviteUserAsync(
                orgId,
                request,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Organisation", orgId));

        var result = await _controller.InviteUser(orgId, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<PendingOrganisationInviteResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ResendInvite_WhenValid_ReturnsNoContent()
    {
        var orgId = Guid.NewGuid();

        var result = await _controller.ResendInvite(orgId, 1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _inviteOrchestrationService.Verify(
            service => service.ResendInviteAsync(orgId, 1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendInvite_WhenNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        _inviteOrchestrationService.Setup(service => service.ResendInviteAsync(orgId, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("PendingOrganisationInvite", 1));

        var result = await _controller.ResendInvite(orgId, 1, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ChangeInviteRole_WhenValid_ReturnsNoContent()
    {
        var orgId = Guid.NewGuid();
        var request = new ChangeOrganisationRoleRequest { OrganisationRole = OrganisationRole.Admin };

        var result = await _controller.ChangeInviteRole(orgId, 3, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _inviteOrchestrationService.Verify(
            service => service.ChangeInviteRoleAsync(orgId, 3, OrganisationRole.Admin, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeInviteRole_WhenNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        var request = new ChangeOrganisationRoleRequest { OrganisationRole = OrganisationRole.Admin };
        _inviteOrchestrationService.Setup(service => service.ChangeInviteRoleAsync(orgId, 3, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("PendingOrganisationInvite", 3));

        var result = await _controller.ChangeInviteRole(orgId, 3, request, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
