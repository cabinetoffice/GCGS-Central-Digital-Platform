using CO.CDP.UserManagement.App.Controllers;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userService;
    private readonly Mock<IInviteUserStateStore> _inviteUserStateStore;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userService = new Mock<IUserService>();
        _inviteUserStateStore = new Mock<IInviteUserStateStore>();
        _inviteUserStateStore.Setup(store => store.ClearAsync()).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.SetAsync(It.IsAny<InviteUserState>())).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync((InviteUserState?)null);
        _controller = new UsersController(_userService.Object, _inviteUserStateStore.Object);
    }

    [Fact]
    public async Task Index_WhenModelStateInvalid_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("error", "invalid");

        var result = await _controller.Index("org", null, null, null, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Index_WhenOrganisationSlugMissing_ReturnsNotFound()
    {
        var result = await _controller.Index(null, null, null, null, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenViewModelNull_ReturnsNotFound()
    {
        _userService.Setup(service => service.GetUsersViewModelAsync("org", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsersViewModel?)null);

        var result = await _controller.Index("org", null, null, null, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = UsersViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetUsersViewModelAsync("org", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Index("org", null, null, null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _userService.Setup(service => service.GetInviteUserViewModelAsync("org", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InviteUserViewModel?)null);

        var result = await _controller.Add("org", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Add_Get_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetInviteUserViewModelAsync("org", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add("org", CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Post_WhenModelStateInvalid_ReturnsView()
    {
        var input = InviteUserViewModel.Empty;
        _controller.ModelState.AddModelError("error", "invalid");
        var viewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetInviteUserViewModelAsync("org", input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add("org", input, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Post_WhenValid_RedirectsToOrganisationRole()
    {
        var input = InviteUserViewModel.Empty with { Email = "user@example.com", FirstName = "First", LastName = "Last" };
        var result = await _controller.Add("org", input, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.OrganisationRoleStep));
    }

    [Fact]
    public async Task OrganisationRole_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync((InviteUserState?)null);

        var result = await _controller.OrganisationRoleStep("org");

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Add));
    }

    [Fact]
    public async Task OrganisationRole_WhenStateAvailable_ReturnsViewWithStateModel()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);

        var result = await _controller.OrganisationRoleStep("org");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("OrganisationRole");
        viewResult.Model.Should().Be(state);
    }

    [Fact]
    public async Task ApplicationRoles_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync((InviteUserState?)null);

        var result = await _controller.ApplicationRolesStep("org");

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Add));
    }

    [Fact]
    public async Task ApplicationRoles_WhenStateAvailable_ReturnsViewWithStateModel()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);

        var result = await _controller.ApplicationRolesStep("org");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ApplicationRoles");
        viewResult.Model.Should().Be(state);
    }

    [Fact]
    public async Task ResendInvite_WhenSuccess_RedirectsToIndex()
    {
        _userService.Setup(service => service.ResendInviteAsync("org", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ResendInvite("org", 1, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
    }

    [Fact]
    public async Task ResendInvite_WhenFails_ReturnsNotFound()
    {
        _userService.Setup(service => service.ResendInviteAsync("org", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.ResendInvite("org", 1, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeRole_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", It.IsAny<Guid?>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserRoleViewModel?)null);

        var result = await _controller.ChangeRole("org", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeRole_Get_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", It.IsAny<Guid?>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeRole("org", Guid.NewGuid(), CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task ChangeRole_Post_WhenRoleMissing_ReturnsView()
    {
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", It.IsAny<Guid?>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeRole("org", Guid.NewGuid(), null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task ChangeRole_Post_WhenSuccess_Redirects()
    {
        _userService.Setup(service => service.UpdateUserRoleAsync("org", It.IsAny<Guid?>(), null, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ChangeRole("org", Guid.NewGuid(), OrganisationRole.Admin, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
    }

    [Fact]
    public async Task ChangeRole_Post_WhenFails_ReturnsNotFound()
    {
        _userService.Setup(service => service.UpdateUserRoleAsync("org", It.IsAny<Guid?>(), null, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.ChangeRole("org", Guid.NewGuid(), OrganisationRole.Admin, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeInviteRole_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserRoleViewModel?)null);

        var result = await _controller.ChangeInviteRole("org", 1, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeInviteRole_Get_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeInviteRole("org", 1, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRole");
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task ChangeInviteRole_Post_WhenRoleMissing_ReturnsView()
    {
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeInviteRole("org", 1, null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRole");
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task ChangeInviteRole_Post_WhenSuccess_Redirects()
    {
        _userService.Setup(service => service.UpdateUserRoleAsync("org", null, 1, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ChangeInviteRole("org", 1, OrganisationRole.Admin, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
    }

    [Fact]
    public async Task ChangeInviteRole_Post_WhenFails_ReturnsNotFound()
    {
        _userService.Setup(service => service.UpdateUserRoleAsync("org", null, 1, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.ChangeInviteRole("org", 1, OrganisationRole.Admin, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
