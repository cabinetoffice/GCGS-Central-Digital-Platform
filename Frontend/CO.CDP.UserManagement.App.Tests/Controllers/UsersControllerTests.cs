using CO.CDP.UserManagement.App.Controllers;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userService;
    private readonly Mock<IInviteUserStateStore> _inviteUserStateStore;
    private readonly Mock<IChangeRoleStateStore> _changeRoleStateStore;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userService = new Mock<IUserService>();
        _inviteUserStateStore = new Mock<IInviteUserStateStore>();
        _changeRoleStateStore = new Mock<IChangeRoleStateStore>();
        _inviteUserStateStore.Setup(store => store.ClearAsync()).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.ClearSuccessAsync()).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.SetAsync(It.IsAny<InviteUserState>())).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.SetSuccessAsync(It.IsAny<InviteSuccessState>())).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync((InviteUserState?)null);
        _inviteUserStateStore.Setup(store => store.GetSuccessAsync()).ReturnsAsync((InviteSuccessState?)null);
        _changeRoleStateStore.Setup(store => store.GetAsync()).ReturnsAsync((ChangeRoleState?)null);
        _changeRoleStateStore.Setup(store => store.SetAsync(It.IsAny<ChangeRoleState>())).Returns(Task.CompletedTask);
        _changeRoleStateStore.Setup(store => store.ClearAsync()).Returns(Task.CompletedTask);
        _controller = new UsersController(_userService.Object, _inviteUserStateStore.Object, _changeRoleStateStore.Object);
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

        var result = await _controller.Add("org", ct: CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Add_Get_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetInviteUserViewModelAsync("org", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add("org", ct: CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Get_WhenSessionStateExists_PrefillsFromState()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last", OrganisationRole.Admin);
        var viewModel = InviteUserViewModel.Empty with
        {
            OrganisationName = "Org",
            Email = state.Email,
            FirstName = state.FirstName,
            LastName = state.LastName,
            OrganisationRole = state.OrganisationRole
        };
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);
        _userService.Setup(service => service.GetInviteUserViewModelAsync(
                "org",
                It.Is<InviteUserViewModel>(vm =>
                    vm.Email == state.Email &&
                    vm.FirstName == state.FirstName &&
                    vm.LastName == state.LastName &&
                    vm.OrganisationRole == state.OrganisationRole),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add("org", ct: CancellationToken.None);

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

        var result = await _controller.Add("org", input, ct: CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Post_WhenValid_RedirectsToOrganisationRole()
    {
        var input = InviteUserViewModel.Empty with { Email = "user@example.com", FirstName = "First", LastName = "Last" };
        var result = await _controller.Add("org", input, ct: CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.OrganisationRoleStep));
    }

    [Fact]
    public async Task Add_Post_WhenReturnToCheckAnswers_RedirectsToCheckAnswers()
    {
        var input = InviteUserViewModel.Empty with { Email = "user@example.com", FirstName = "First", LastName = "Last" };

        var result = await _controller.Add("org", input, returnToCheckAnswers: true, ct: CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.CheckAnswersStep));
    }

    [Fact]
    public async Task Add_Post_WhenValidAndSessionExists_PreservesRoleAndAssignments()
    {
        var existingState = new InviteUserState(
            "org",
            "old@example.com",
            "Old",
            "Name",
            OrganisationRole.Owner,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 5 }]);
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(existingState);
        var input = InviteUserViewModel.Empty with { Email = "user@example.com", FirstName = "First", LastName = "Last" };

        await _controller.Add("org", input, ct: CancellationToken.None);

        _inviteUserStateStore.Verify(store => store.SetAsync(
            It.Is<InviteUserState>(s =>
                s.OrganisationRole == OrganisationRole.Owner &&
                s.ApplicationAssignments != null &&
                s.ApplicationAssignments.Count == 1 &&
                s.ApplicationAssignments[0].OrganisationApplicationId == 10 &&
                s.ApplicationAssignments[0].ApplicationRoleId == 5)),
            Times.Once);
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
    public async Task OrganisationRole_WhenReturnToCheckAnswers_SetsViewDataFlag()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);

        var result = await _controller.OrganisationRoleStep("org", returnToCheckAnswers: true);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewData["ReturnToCheckAnswers"].Should().Be(true);
    }

    [Fact]
    public async Task ApplicationRoles_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync((InviteUserState?)null);

        var result = await _controller.ApplicationRolesStep("org", organisationRole: null, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Add));
    }

    [Fact]
    public async Task ApplicationRoles_WhenStateAvailable_ReturnsViewWithViewModel()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        var viewModel = new ApplicationRolesStepViewModel
        {
            OrganisationSlug = "org",
            FirstName = "First",
            LastName = "Last",
            Email = "user@example.com",
            Applications = []
        };
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);
        _userService.Setup(service => service.GetApplicationRolesStepViewModelAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ApplicationRolesStep("org", organisationRole: null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ApplicationRoles");
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task ApplicationRoles_Post_WhenNoSelection_ReturnsViewWithModelError()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        var viewModel = new ApplicationRolesStepViewModel
        {
            OrganisationSlug = "org",
            FirstName = "First",
            LastName = "Last",
            Email = "user@example.com",
            Applications =
            [
                new()
                {
                    OrganisationApplicationId = 10,
                    ApplicationName = "Payments",
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Admin" }]
                }
            ]
        };
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);
        _userService.Setup(service => service.GetApplicationRolesStepViewModelAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ApplicationRolesStepSubmit(
            "org",
            new ApplicationRolesStepPostModel { Applications = [new ApplicationSelectionPostModel { OrganisationApplicationId = 10 }] },
            CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ApplicationRoles");
        _controller.ModelState.ContainsKey("applicationSelections").Should().BeTrue();
    }

    [Fact]
    public async Task ApplicationRoles_Post_WhenValidSelection_RedirectsToCheckAnswers()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        var viewModel = new ApplicationRolesStepViewModel
        {
            OrganisationSlug = "org",
            FirstName = "First",
            LastName = "Last",
            Email = "user@example.com",
            Applications =
            [
                new()
                {
                    OrganisationApplicationId = 10,
                    ApplicationName = "Payments",
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Admin" }]
                }
            ]
        };
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);
        _userService.Setup(service => service.GetApplicationRolesStepViewModelAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        var result = await _controller.ApplicationRolesStepSubmit(
            "org",
            new ApplicationRolesStepPostModel
            {
                Applications =
                [
                    new ApplicationSelectionPostModel
                    {
                        OrganisationApplicationId = 10,
                        GiveAccess = true,
                        SelectedRoleId = 5
                    }
                ]
            },
            CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.CheckAnswersStep));
    }

    [Fact]
    public async Task CheckAnswers_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync((InviteUserState?)null);

        var result = await _controller.CheckAnswersStep("org", null, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Add));
    }

    [Fact]
    public async Task CheckAnswers_WhenValidState_ReturnsView()
    {
        var state = new InviteUserState(
            "org",
            "user@example.com",
            "First",
            "Last",
            OrganisationRole.Admin,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 5 }]);
        var rolesViewModel = new ApplicationRolesStepViewModel
        {
            OrganisationSlug = "org",
            Applications =
            [
                new()
                {
                    OrganisationApplicationId = 10,
                    ApplicationName = "Payments",
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Admin" }]
                }
            ]
        };
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);
        _userService.Setup(service => service.GetApplicationRolesStepViewModelAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolesViewModel);

        var result = await _controller.CheckAnswersStep("org", null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("CheckAnswers");
    }

    [Fact]
    public async Task CheckAnswers_Post_WhenValid_InvitesAndRedirectsToSuccess()
    {
        var state = new InviteUserState(
            "org",
            "user@example.com",
            "First",
            "Last",
            OrganisationRole.Admin,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 5 }]);
        var rolesViewModel = new ApplicationRolesStepViewModel
        {
            OrganisationSlug = "org",
            Applications =
            [
                new()
                {
                    OrganisationApplicationId = 10,
                    ApplicationName = "Payments",
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Admin" }]
                }
            ]
        };
        var invitePageViewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);
        _userService.Setup(service => service.GetApplicationRolesStepViewModelAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolesViewModel);
        _userService.Setup(service => service.GetInviteUserViewModelAsync("org", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitePageViewModel);
        _userService.Setup(service => service.InviteUserAsync(
                "org",
                It.IsAny<InviteUserViewModel>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<IReadOnlyList<InviteApplicationAssignment>>()))
            .ReturnsAsync(true);

        var result = await _controller.CheckAnswersStepSubmit("org", CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.InviteSuccessStep));
    }

    [Fact]
    public async Task InviteSuccess_WhenStateMissing_RedirectsToIndex()
    {
        _inviteUserStateStore.Setup(store => store.GetSuccessAsync()).ReturnsAsync((InviteSuccessState?)null);

        var result = await _controller.InviteSuccessStep("org");

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
    }

    [Fact]
    public async Task InviteSuccess_WhenStateAvailable_ReturnsView()
    {
        var successState = new InviteSuccessState
        {
            OrganisationSlug = "org",
            OrganisationName = "Org",
            FirstName = "First",
            LastName = "Last",
            Email = "user@example.com",
            DateAdded = DateTimeOffset.UtcNow,
            Applications = [new InviteSuccessApplicationRoleViewModel { ApplicationName = "Payments", RoleName = "Admin" }]
        };
        _inviteUserStateStore.Setup(store => store.GetSuccessAsync()).ReturnsAsync(successState);

        var result = await _controller.InviteSuccessStep("org");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("InviteSuccess");
        viewResult.Model.Should().BeOfType<InviteSuccessViewModel>();
    }

    [Fact]
    public async Task ResendInvite_WhenSuccess_RedirectsToIndex()
    {
        var inviteGuid = Guid.NewGuid();
        _userService.Setup(service => service.ResendInviteAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ResendInvite("org", inviteGuid, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
    }

    [Fact]
    public async Task ResendInvite_WhenFails_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _userService.Setup(service => service.ResendInviteAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.ResendInvite("org", inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_WhenViewModelNull_ReturnsNotFound()
    {
        _userService.Setup(service => service.GetUserDetailsViewModelAsync("org", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDetailsViewModel?)null);

        var result = await _controller.Details("org", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_WhenViewModelAvailable_ReturnsView()
    {
        var applicationAccess = new[]
        {
            new UserApplicationAccessDetailViewModel(
                ApplicationId: 1,
                ApplicationName: "Edit",
                ApplicationDescription: "Edit application",
                Permissions: new[] { "Read", "Write" },
                AssignedDate: DateTimeOffset.UtcNow,
                AssignedByEmail: "admin@example.com",
                ApplicationRole: "Admin"),
            new UserApplicationAccessDetailViewModel(
                ApplicationId: 2,
                ApplicationName: "View",
                ApplicationDescription: "View application",
                Permissions: new[] { "Read" },
                AssignedDate: DateTimeOffset.UtcNow.AddDays(-1),
                AssignedByEmail: "admin@example.com",
                ApplicationRole: "Editor")
        };
        var organisation = new OrganisationResponse
        {
            Id = 1,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Org",
            Slug = "org",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var viewModel = new UserDetailsViewModel(
            Organisation: organisation,
            CdpPersonId: Guid.NewGuid(),
            FullName: "Test User",
            Email: "test@example.com",
            OrganisationRole: OrganisationRole.Admin,
            MemberSince: "19 February 2026",
            ApplicationAccess: applicationAccess);
        _userService.Setup(service => service.GetUserDetailsViewModelAsync("org", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Details("org", Guid.NewGuid(), CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
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
    public async Task ChangeRole_Get_WhenSessionStateExists_ReturnsViewWithPersistedRole()
    {
        var userId = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationSlug = "org",
            CdpPersonId = userId,
            SelectedRole = OrganisationRole.Member
        };
        var persistedState = new ChangeRoleState(
            "org",
            userId,
            null,
            "Jane Doe",
            "jane@example.com",
            OrganisationRole.Member,
            OrganisationRole.Admin);
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", It.IsAny<Guid?>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _changeRoleStateStore.Setup(store => store.GetAsync()).ReturnsAsync(persistedState);

        var result = await _controller.ChangeRole("org", userId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRole");
        viewResult.Model.Should().BeOfType<ChangeUserRoleViewModel>()
            .Which.SelectedRole.Should().Be(OrganisationRole.Admin);
    }

    [Fact]
    public async Task ChangeRole_Post_WhenRoleMissing_ReturnsView()
    {
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", It.IsAny<Guid?>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeRoleSubmit("org", Guid.NewGuid(), null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRole");
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task ChangeRole_Post_WhenRoleProvided_RedirectsToCheck()
    {
        var userId = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationSlug = "org",
            CdpPersonId = userId,
            SelectedRole = OrganisationRole.Member
        };
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeRoleSubmit("org", userId, OrganisationRole.Admin, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeRoleCheck));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
        redirect.RouteValues.ContainsKey("organisationRole").Should().BeFalse();
        _userService.Verify(service => service.UpdateUserRoleAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<OrganisationRole>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeRoleCheck_Get_WhenSessionMissing_RedirectsToChangeRole()
    {
        var userId = Guid.NewGuid();

        var result = await _controller.ChangeRoleCheck("org", userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeRole));
    }

    [Fact]
    public async Task ChangeRoleCheck_Post_WhenSuccess_RedirectsToSuccess()
    {
        var userId = Guid.NewGuid();
        _changeRoleStateStore.Setup(store => store.GetAsync()).ReturnsAsync(new ChangeRoleState(
            "org",
            userId,
            null,
            "Jane Doe",
            "jane@example.com",
            OrganisationRole.Member,
            OrganisationRole.Admin));
        _userService.Setup(service => service.UpdateUserRoleAsync("org", userId, null, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ChangeRoleCheckSubmit("org", userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeRoleSuccess));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
    }

    [Fact]
    public async Task ChangeRoleCheck_Post_WhenFails_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        _changeRoleStateStore.Setup(store => store.GetAsync()).ReturnsAsync(new ChangeRoleState(
            "org",
            userId,
            null,
            "Jane Doe",
            "jane@example.com",
            OrganisationRole.Member,
            OrganisationRole.Admin));
        _userService.Setup(service => service.UpdateUserRoleAsync("org", userId, null, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.ChangeRoleCheckSubmit("org", userId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeRoleSuccess_Get_WhenValid_ReturnsView()
    {
        var userId = Guid.NewGuid();
        _changeRoleStateStore.Setup(store => store.GetAsync()).ReturnsAsync(new ChangeRoleState(
            "org",
            userId,
            null,
            "Jane Doe",
            "jane@example.com",
            OrganisationRole.Member,
            OrganisationRole.Owner));

        var result = await _controller.ChangeRoleSuccess("org", userId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRoleSuccess");
        viewResult.Model.Should().BeOfType<ChangeUserRoleSuccessViewModel>()
            .Which.NewRole.Should().Be(OrganisationRole.Owner);
        _changeRoleStateStore.Verify(store => store.ClearAsync(), Times.Once);
    }

    [Fact]
    public async Task ChangeInviteRole_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", null, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserRoleViewModel?)null);

        var result = await _controller.ChangeInviteRole("org", inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeInviteRole_Get_WhenViewModelAvailable_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org", InviteGuid = inviteGuid };
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", null, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeInviteRole("org", inviteGuid, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRole");
        viewResult.Model.Should().BeOfType<ChangeUserRoleViewModel>()
            .Which.SelectedRole.Should().Be(OrganisationRole.Member);
    }

    [Fact]
    public async Task ChangeInviteRole_Post_WhenRoleMissing_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org", InviteGuid = inviteGuid };
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", null, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeInviteRoleSubmit("org", inviteGuid, null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRole");
        viewResult.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task ChangeInviteRole_Post_WhenRoleProvided_RedirectsToCheck()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationSlug = "org", InviteGuid = inviteGuid };
        _userService.Setup(service => service.GetChangeUserRoleViewModelAsync("org", null, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeInviteRoleSubmit("org", inviteGuid, OrganisationRole.Admin, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeInviteRoleCheck));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    [Fact]
    public async Task ChangeInviteRoleCheck_Post_WhenSuccess_RedirectsToSuccess()
    {
        var inviteGuid = Guid.NewGuid();
        _changeRoleStateStore.Setup(store => store.GetAsync()).ReturnsAsync(new ChangeRoleState(
            "org",
            null,
            inviteGuid,
            "Jane Invite",
            "jane@example.com",
            OrganisationRole.Member,
            OrganisationRole.Admin));
        _userService.Setup(service => service.UpdateUserRoleAsync("org", null, inviteGuid, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ChangeInviteRoleCheckSubmit("org", inviteGuid, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeInviteRoleSuccess));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    [Fact]
    public async Task ChangeInviteRoleSuccess_Get_WhenValid_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        _changeRoleStateStore.Setup(store => store.GetAsync()).ReturnsAsync(new ChangeRoleState(
            "org",
            null,
            inviteGuid,
            "Jane Invite",
            "jane@example.com",
            OrganisationRole.Member,
            OrganisationRole.Member));

        var result = await _controller.ChangeInviteRoleSuccess("org", inviteGuid, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRoleSuccess");
        viewResult.Model.Should().BeOfType<ChangeUserRoleSuccessViewModel>()
            .Which.NewRole.Should().Be(OrganisationRole.Member);
        _changeRoleStateStore.Verify(store => store.ClearAsync(), Times.Once);
    }
}
