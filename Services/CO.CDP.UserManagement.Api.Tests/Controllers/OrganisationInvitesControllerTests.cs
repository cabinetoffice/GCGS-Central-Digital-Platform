using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using IOrganisationRepository = CO.CDP.UserManagement.Core.Interfaces.IOrganisationRepository;

namespace CO.CDP.UserManagement.Api.Tests.Controllers;

public class OrganisationInvitesControllerTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepository;
    private readonly Mock<IInviteRoleMappingRepository> _inviteRoleMappingRepository;
    private readonly OrganisationInformationContext _organisationInformationContext;
    private readonly Mock<IInviteOrchestrationService> _inviteOrchestrationService;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly OrganisationInvitesController _controller;

    public OrganisationInvitesControllerTests()
    {
        _organisationRepository = new Mock<IOrganisationRepository>();
        _inviteRoleMappingRepository = new Mock<IInviteRoleMappingRepository>();
        var options = new DbContextOptionsBuilder<OrganisationInformationContext>()
            .UseInMemoryDatabase($"OrganisationInvites-{Guid.NewGuid()}")
            .Options;
        _organisationInformationContext = new OrganisationInformationContext(options);
        _inviteOrchestrationService = new Mock<IInviteOrchestrationService>();
        _currentUserService = new Mock<ICurrentUserService>();

        _controller = new OrganisationInvitesController(
            _organisationRepository.Object,
            _inviteRoleMappingRepository.Object,
            _organisationInformationContext,
            _inviteOrchestrationService.Object,
            _currentUserService.Object);
    }

    [Fact]
    public async Task GetInvites_WhenOrganisationNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        _organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoreOrganisation?)null);

        var result = await _controller.GetInvites(orgId, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<PendingOrganisationInviteResponse>>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetInvites_WhenOrganisationFound_ReturnsOk()
    {
        var orgId = Guid.NewGuid();
        var organisation = new CoreOrganisation { Id = 12, CdpOrganisationGuid = orgId, Name = "Org", Slug = "org" };
        var cdpInviteGuid = Guid.NewGuid();
        var inviteRoleMappings = new List<InviteRoleMapping>
        {
            new()
            {
                Id = 5,
                OrganisationId = organisation.Id,
                OrganisationRole = OrganisationRole.Admin,
                CdpPersonInviteGuid = cdpInviteGuid,
                CreatedBy = "inviter",
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }
        };
        _organisationRepository.Setup(repo => repo.GetByCdpGuidAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _inviteRoleMappingRepository.Setup(repo => repo.GetByOrganisationIdAsync(organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inviteRoleMappings);

        _organisationInformationContext.PersonInvites.Add(new PersonInvite
        {
            Guid = cdpInviteGuid,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            OrganisationId = 1,
            Scopes = new List<string> { "ADMIN" },
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        });
        await _organisationInformationContext.SaveChangesAsync();

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
        var invite = new InviteRoleMapping
        {
            Id = 9,
            OrganisationId = 12,
            OrganisationRole = request.OrganisationRole,
            CdpPersonInviteGuid = Guid.NewGuid(),
            CreatedBy = "inviter",
            CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _currentUserService.Setup(service => service.GetUserPrincipalId()).Returns("inviter");
        _inviteOrchestrationService.Setup(service => service.InviteUserAsync(
                orgId,
                request,
                "inviter",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        _organisationInformationContext.PersonInvites.Add(new PersonInvite
        {
            Guid = invite.CdpPersonInviteGuid,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            OrganisationId = 1,
            Scopes = new List<string> { "ADMIN" },
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        });
        await _organisationInformationContext.SaveChangesAsync();

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
            .ThrowsAsync(new DuplicateEntityException("InviteRoleMapping", "Email", request.Email));

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
            .ThrowsAsync(new EntityNotFoundException("InviteRoleMapping", 3));

        var result = await _controller.ChangeInviteRole(orgId, 3, request, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
