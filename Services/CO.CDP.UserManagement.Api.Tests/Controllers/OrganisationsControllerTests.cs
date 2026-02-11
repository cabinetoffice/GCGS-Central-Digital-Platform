using CO.CDP.UserManagement.Api.Controllers;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.UnitTests.Controllers;

public class OrganisationsControllerTests
{
    private readonly Mock<IOrganisationService> _organisationService;
    private readonly OrganisationsController _controller;

    public OrganisationsControllerTests()
    {
        _organisationService = new Mock<IOrganisationService>();
        var logger = new Mock<ILogger<OrganisationsController>>();
        _controller = new OrganisationsController(_organisationService.Object, logger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithOrganisations()
    {
        var organisations = new List<Organisation>
        {
            new()
            {
                Id = 1,
                CdpOrganisationGuid = Guid.NewGuid(),
                Name = "Org One",
                Slug = "org-one",
                IsActive = true,
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }
        };
        _organisationService.Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisations);

        var result = await _controller.GetAll(CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<OrganisationResponse>>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IEnumerable<OrganisationResponse>>().Subject.ToList();
        response.Should().HaveCount(1);
        response[0].Name.Should().Be("Org One");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _organisationService.Setup(service => service.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organisation?)null);

        var result = await _controller.GetById(99, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var organisation = new Organisation
        {
            Id = 2,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Org Two",
            Slug = "org-two",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _organisationService.Setup(service => service.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);

        var result = await _controller.GetById(2, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<OrganisationResponse>().Subject;
        response.Id.Should().Be(2);
    }

    [Fact]
    public async Task GetBySlug_WhenNotFound_ReturnsNotFound()
    {
        _organisationService.Setup(service => service.GetBySlugAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organisation?)null);

        var result = await _controller.GetBySlug("missing", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetBySlug_WhenFound_ReturnsOk()
    {
        var organisation = new Organisation
        {
            Id = 3,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Org Three",
            Slug = "org-three",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _organisationService.Setup(service => service.GetBySlugAsync("org-three", It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);

        var result = await _controller.GetBySlug("org-three", CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<OrganisationResponse>().Subject;
        response.Slug.Should().Be("org-three");
    }

    [Fact]
    public async Task GetByCdpGuid_WhenNotFound_ReturnsNotFound()
    {
        var guid = Guid.NewGuid();
        _organisationService.Setup(service => service.GetByCdpGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organisation?)null);

        var result = await _controller.GetByCdpGuid(guid, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByCdpGuid_WhenFound_ReturnsOk()
    {
        var guid = Guid.NewGuid();
        var organisation = new Organisation
        {
            Id = 4,
            CdpOrganisationGuid = guid,
            Name = "Org Four",
            Slug = "org-four",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _organisationService.Setup(service => service.GetByCdpGuidAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);

        var result = await _controller.GetByCdpGuid(guid, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<OrganisationResponse>().Subject;
        response.CdpOrganisationGuid.Should().Be(guid);
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreated()
    {
        var request = new CreateOrganisationRequest
        {
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Org Five",
            IsActive = true
        };
        var organisation = new Organisation
        {
            Id = 5,
            CdpOrganisationGuid = request.CdpOrganisationGuid,
            Name = request.Name,
            Slug = "org-five",
            IsActive = request.IsActive,
            CreatedAt = new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _organisationService.Setup(service => service.CreateAsync(
                request.CdpOrganisationGuid,
                request.Name,
                request.IsActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);

        var result = await _controller.Create(request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(OrganisationsController.GetById));
        created.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(5);
        var response = created.Value.Should().BeOfType<OrganisationResponse>().Subject;
        response.Name.Should().Be("Org Five");
    }

    [Fact]
    public async Task Create_WhenDuplicate_ReturnsBadRequest()
    {
        var request = new CreateOrganisationRequest
        {
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Org Five",
            IsActive = true
        };
        _organisationService.Setup(service => service.CreateAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEntityException("Organisation", "CdpOrganisationGuid", request.CdpOrganisationGuid));

        var result = await _controller.Create(request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var error = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        error.Code.Should().Be("DUPLICATE_ENTITY");
    }

    [Fact]
    public async Task Update_WhenValid_ReturnsOk()
    {
        var request = new UpdateOrganisationRequest
        {
            Name = "Updated",
            IsActive = false
        };
        var organisation = new Organisation
        {
            Id = 6,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = request.Name,
            Slug = "org-six",
            IsActive = request.IsActive,
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _organisationService.Setup(service => service.UpdateAsync(
                6,
                request.Name,
                request.IsActive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);

        var result = await _controller.Update(6, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<OrganisationResponse>().Subject;
        response.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var request = new UpdateOrganisationRequest
        {
            Name = "Updated",
            IsActive = true
        };
        _organisationService.Setup(service => service.UpdateAsync(
                6,
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Organisation", 6));

        var result = await _controller.Update(6, request, CancellationToken.None);

        var actionResult = result.Should().BeOfType<ActionResult<OrganisationResponse>>().Subject;
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenValid_ReturnsNoContent()
    {
        var result = await _controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _organisationService.Verify(service => service.DeleteAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _organisationService.Setup(service => service.DeleteAsync(7, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Organisation", 7));

        var result = await _controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
