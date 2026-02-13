using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Controllers;

public class OrganisationUsersControllerTests
{
    private readonly Mock<IOrganisationUserService> _organisationUserService;
    private readonly Mock<IPersonLookupService> _personLookupService;
    private readonly OrganisationUsersController _controller;

    public OrganisationUsersControllerTests()
    {
        _organisationUserService = new Mock<IOrganisationUserService>();
        _personLookupService = new Mock<IPersonLookupService>();
        var logger = new Mock<ILogger<OrganisationUsersController>>();
        _controller = new OrganisationUsersController(
            _organisationUserService.Object,
            _personLookupService.Object,
            logger.Object);
    }

    [Fact]
    public async Task GetUsers_WhenValid_ReturnsOkWithPersonDetails()
    {
        var orgId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var membership = new UserOrganisationMembership
        {
            Id = 1,
            OrganisationId = 10,
            UserPrincipalId = "user-1",
            CdpPersonId = personId,
            OrganisationRole = OrganisationRole.Admin,
            IsActive = true,
            JoinedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
        var personDetails = new PersonDetails
        {
            CdpPersonId = personId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com"
        };
        _organisationUserService.Setup(service => service.GetOrganisationUsersAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { membership });
        _personLookupService.Setup(service => service.GetPersonDetailsByIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, PersonDetails> { [personId] = personDetails });

        var result = await _controller.GetUsers(orgId, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<OrganisationUserResponse>>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<OrganisationUserResponse>>().Subject.ToList();
        response.Should().HaveCount(1);
        response[0].Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetUsers_WhenNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        _organisationUserService.Setup(service => service.GetOrganisationUsersAsync(orgId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Organisation", orgId));

        var result = await _controller.GetUsers(orgId, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<OrganisationUserResponse>>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUsersForService_WhenValid_ReturnsOk()
    {
        var orgId = Guid.NewGuid();
        var membership = new UserOrganisationMembership
        {
            Id = 2,
            OrganisationId = 10,
            UserPrincipalId = "user-2",
            OrganisationRole = OrganisationRole.Member,
            IsActive = true,
            JoinedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero),
            ApplicationAssignments = new List<UserApplicationAssignment>
            {
                new()
                {
                    Id = 1,
                    UserOrganisationMembershipId = 2,
                    OrganisationApplicationId = 3,
                    IsActive = true,
                    CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)
                }
            }
        };
        _organisationUserService.Setup(service => service.GetOrganisationUsersAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { membership });

        var result = await _controller.GetUsersForService(orgId, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<OrganisationUserResponse>>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<OrganisationUserResponse>>().Subject.ToList();
        response.Should().HaveCount(1);
        response[0].ApplicationAssignments.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsersForService_WhenNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        _organisationUserService.Setup(service => service.GetOrganisationUsersAsync(orgId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Organisation", orgId));

        var result = await _controller.GetUsersForService(orgId, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<OrganisationUserResponse>>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserByPersonId_WhenNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        _organisationUserService.Setup(service => service.GetOrganisationUserByPersonIdAsync(orgId, personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);

        var result = await _controller.GetUserByPersonId(orgId, personId, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationUserResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserByPersonId_WhenValid_ReturnsOk()
    {
        var orgId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var membership = new UserOrganisationMembership
        {
            Id = 3,
            OrganisationId = 10,
            UserPrincipalId = "user-3",
            CdpPersonId = personId,
            OrganisationRole = OrganisationRole.Admin,
            IsActive = true,
            JoinedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _organisationUserService.Setup(service => service.GetOrganisationUserByPersonIdAsync(orgId, personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);
        _personLookupService.Setup(service => service.GetPersonDetailsByIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, PersonDetails>
            {
                [personId] = new PersonDetails
                {
                    CdpPersonId = personId,
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com"
                }
            });

        var result = await _controller.GetUserByPersonId(orgId, personId, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationUserResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<OrganisationUserResponse>().Subject;
        response.CdpPersonId.Should().Be(personId);
    }

    [Fact]
    public async Task UpdateRole_WhenValid_ReturnsNoContent()
    {
        var orgId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var request = new ChangeOrganisationRoleRequest { OrganisationRole = OrganisationRole.Admin };

        var result = await _controller.UpdateRole(orgId, personId, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _organisationUserService.Verify(service => service.UpdateOrganisationRoleAsync(
            orgId,
            personId,
            OrganisationRole.Admin,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRole_WhenNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var request = new ChangeOrganisationRoleRequest { OrganisationRole = OrganisationRole.Admin };
        _organisationUserService.Setup(service => service.UpdateOrganisationRoleAsync(orgId, personId, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("UserOrganisationMembership", personId));

        var result = await _controller.UpdateRole(orgId, personId, request, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUser_WhenNotFound_ReturnsNotFound()
    {
        var orgId = Guid.NewGuid();
        _organisationUserService.Setup(service => service.GetOrganisationUserAsync(orgId, "user-4", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);

        var result = await _controller.GetUser(orgId, "user-4", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationUserResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUser_WhenValid_ReturnsOk()
    {
        var orgId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var membership = new UserOrganisationMembership
        {
            Id = 4,
            OrganisationId = 10,
            UserPrincipalId = "user-4",
            CdpPersonId = personId,
            OrganisationRole = OrganisationRole.Member,
            IsActive = true,
            JoinedAt = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _organisationUserService.Setup(service => service.GetOrganisationUserAsync(orgId, "user-4", It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);
        _personLookupService.Setup(service => service.GetPersonDetailsByIdsAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, PersonDetails>
            {
                [personId] = new PersonDetails
                {
                    CdpPersonId = personId,
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com"
                }
            });

        var result = await _controller.GetUser(orgId, "user-4", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationUserResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<OrganisationUserResponse>().Subject;
        response.Email.Should().Be("test@example.com");
    }
}
