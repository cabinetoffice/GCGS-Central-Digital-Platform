using CO.CDP.UserManagement.App.Controllers;
using CO.CDP.Functional;
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
    private readonly Mock<IOrganisationRoleService> _organisationRoleService;
    private readonly Mock<IInviteUserStateStore> _inviteUserStateStore;
    private readonly Mock<IChangeRoleStateStore> _changeRoleStateStore;
    private readonly Mock<IChangeApplicationRoleStateStore> _changeApplicationRoleStateStore;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userService = new Mock<IUserService>();
        _organisationRoleService = new Mock<IOrganisationRoleService>();
        _inviteUserStateStore = new Mock<IInviteUserStateStore>();
        _changeRoleStateStore = new Mock<IChangeRoleStateStore>();
        _changeApplicationRoleStateStore = new Mock<IChangeApplicationRoleStateStore>();
        _inviteUserStateStore.Setup(store => store.ClearAsync()).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.ClearSuccessAsync()).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.SetAsync(It.IsAny<InviteUserState>())).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.SetSuccessAsync(It.IsAny<InviteSuccessState>())).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync((InviteUserState?)null);
        _inviteUserStateStore.Setup(store => store.GetSuccessAsync()).ReturnsAsync((InviteSuccessState?)null);
        _changeRoleStateStore.Setup(store => store.GetAsync()).ReturnsAsync((ChangeRoleState?)null);
        _changeRoleStateStore.Setup(store => store.SetAsync(It.IsAny<ChangeRoleState>())).Returns(Task.CompletedTask);
        _changeRoleStateStore.Setup(store => store.ClearAsync()).Returns(Task.CompletedTask);
        _changeApplicationRoleStateStore.Setup(store => store.GetAsync()).ReturnsAsync((ChangeApplicationRoleState?)null);
        _changeApplicationRoleStateStore.Setup(store => store.SetAsync(It.IsAny<ChangeApplicationRoleState>())).Returns(Task.CompletedTask);
        _changeApplicationRoleStateStore.Setup(store => store.ClearAsync()).Returns(Task.CompletedTask);
        var roles = new[]
        {
            new CO.CDP.UserManagement.Shared.Responses.OrganisationRoleDefinitionResponse { Id = OrganisationRole.Member, DisplayName = "Member", Description = "Member description" },
            new CO.CDP.UserManagement.Shared.Responses.OrganisationRoleDefinitionResponse { Id = OrganisationRole.Admin, DisplayName = "Admin", Description = "Admin description" },
            new CO.CDP.UserManagement.Shared.Responses.OrganisationRoleDefinitionResponse { Id = OrganisationRole.Owner, DisplayName = "Owner", Description = "Owner description" }
        };
        _organisationRoleService.Setup(service => service.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _organisationRoleService.Setup(service => service.GetRoleAsync(It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganisationRole role, CancellationToken _) => roles.FirstOrDefault(r => r.Id == role));
        _controller = new UsersController(
            _userService.Object,
            _organisationRoleService.Object,
            _inviteUserStateStore.Object,
            _changeRoleStateStore.Object,
            _changeApplicationRoleStateStore.Object);
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
        viewResult.Model.Should().BeOfType<OrganisationRoleStepViewModel>()
            .Which.OrganisationSlug.Should().Be("org");
    }

    [Fact]
    public async Task OrganisationRole_WhenReturnToCheckAnswers_ReturnsTypedModelFlag()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        _inviteUserStateStore.Setup(store => store.GetAsync()).ReturnsAsync(state);
        var result = await _controller.OrganisationRoleStep("org", returnToCheckAnswers: true);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<OrganisationRoleStepViewModel>()
            .Which.ReturnToCheckAnswers.Should().BeTrue();
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
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

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
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.ResendInvite("org", inviteGuid, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
    }

    [Fact]
    public async Task ResendInvite_WhenFails_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _userService.Setup(service => service.ResendInviteAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));

        var result = await _controller.ResendInvite("org", inviteGuid, CancellationToken.None);

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
        viewResult.Model.Should().BeOfType<ChangeUserRolePageViewModel>()
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
        viewResult.Model.Should().BeEquivalentTo(
            ChangeUserRolePageViewModel.From(
                viewModel,
                _organisationRoleService.Object.GetRolesAsync(CancellationToken.None).Result.ToOptions()));
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
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

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
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));

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
        viewResult.Model.Should().BeOfType<ChangeUserRolePageViewModel>()
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
        viewResult.Model.Should().BeEquivalentTo(
            ChangeUserRolePageViewModel.From(
                viewModel,
                _organisationRoleService.Object.GetRolesAsync(CancellationToken.None).Result.ToOptions()));
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
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

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

    // ─── ChangeApplicationRoles (user) ───────────────────────────────────────

    [Fact]
    public async Task ChangeApplicationRoles_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", It.IsAny<Guid?>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserApplicationRolesViewModel?)null);

        var result = await _controller.ChangeApplicationRoles("org", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeApplicationRoles_Get_WhenViewModelAvailable_ReturnsView()
    {
        var userId = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel { OrganisationSlug = "org", UserDisplayName = "Jane Doe", Email = "jane@example.com" };
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeApplicationRoles("org", userId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("ChangeApplicationRoles");
        view.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Get_RestoresSelectionsFromSessionState()
    {
        var userId = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org",
            CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel { OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 10, Roles = [new ApplicationRoleOptionViewModel { Id = 10 }, new ApplicationRoleOptionViewModel { Id = 20 }] }
            ]
        };
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 10, "Reader", 20, "Admin")]);

        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.ChangeApplicationRoles("org", userId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var vm = view.Model.Should().BeOfType<ChangeUserApplicationRolesViewModel>().Subject;
        vm.Applications[0].SelectedRoleId.Should().Be(20);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNoChanges_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel> { new() { Id = 5, Name = "Reader" } };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org",
            CdpPersonId = userId,
            Applications = [new ApplicationRoleChangeViewModel { OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 5, Roles = roles }]
        };
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        // Post same role ID that is currently set
        var input = new ApplicationRoleChangePostModel
        {
            Applications = [new ApplicationRoleAssignmentPostModel { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 5 }]
        };

        var result = await _controller.ChangeApplicationRolesSubmit("org", userId, input, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("ChangeApplicationRoles");
        _controller.ModelState.ContainsKey("Applications").Should().BeTrue();
        _changeApplicationRoleStateStore.Verify(s => s.SetAsync(It.IsAny<ChangeApplicationRoleState>()), Times.Never);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenChanged_SavesStateAndRedirectsToCheck()
    {
        var userId = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel>
        {
            new() { Id = 5, Name = "Reader" },
            new() { Id = 6, Name = "Admin" }
        };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org",
            CdpPersonId = userId,
            Applications = [new ApplicationRoleChangeViewModel { OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 5, Roles = roles }]
        };
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var input = new ApplicationRoleChangePostModel
        {
            Applications = [new ApplicationRoleAssignmentPostModel { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 6 }]
        };

        var result = await _controller.ChangeApplicationRolesSubmit("org", userId, input, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeApplicationRolesCheck));
        _changeApplicationRoleStateStore.Verify(s => s.SetAsync(It.Is<ChangeApplicationRoleState>(
            st => st.OrganisationSlug == "org" && st.CdpPersonId == userId)), Times.Once);
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Get_WhenNoState_RedirectsToSelection()
    {
        var userId = Guid.NewGuid();

        var result = await _controller.ChangeApplicationRolesCheck("org", userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeApplicationRoles));
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Get_WhenStateValid_ReturnsCheckView()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane Doe", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.ChangeApplicationRolesCheck("org", userId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("CheckApplicationRoles");
        var vm = view.Model.Should().BeOfType<ChangeApplicationRolesCheckViewModel>().Subject;
        vm.ChangedApplications.Should().ContainSingle(a => a.ApplicationName == "App1" && a.CurrentRoleName == "Reader" && a.NewRoleName == "Admin");
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Post_WhenNoState_RedirectsToSelection()
    {
        var userId = Guid.NewGuid();

        var result = await _controller.ChangeApplicationRolesCheckSubmit("org", userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeApplicationRoles));
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Post_WhenUpdateFails_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _userService.Setup(s => s.UpdateUserApplicationRolesAsync("org", userId, null, It.IsAny<IReadOnlyList<ApplicationRoleAssignmentPostModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));

        var result = await _controller.ChangeApplicationRolesCheckSubmit("org", userId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Post_WhenUpdateSucceeds_RedirectsToSuccess()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _userService.Setup(s => s.UpdateUserApplicationRolesAsync("org", userId, null, It.IsAny<IReadOnlyList<ApplicationRoleAssignmentPostModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.ChangeApplicationRolesCheckSubmit("org", userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeApplicationRolesSuccess));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
    }

    [Fact]
    public async Task ChangeApplicationRolesSuccess_Get_WhenStateValid_ReturnsSuccessViewAndClearsState()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane Doe", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.ChangeApplicationRolesSuccess("org", userId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("ChangeApplicationRolesSuccess");
        view.Model.Should().BeOfType<ChangeApplicationRolesSuccessViewModel>()
            .Which.UserDisplayName.Should().Be("Jane Doe");
        _changeApplicationRoleStateStore.Verify(s => s.ClearAsync(), Times.Once);
    }

    // ─── ChangeApplicationRoles (invite) ─────────────────────────────────────

    [Fact]
    public async Task ChangeInviteApplicationRoles_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", null, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserApplicationRolesViewModel?)null);

        var result = await _controller.ChangeInviteApplicationRoles("org", inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeInviteApplicationRoles_Post_WhenChanged_SavesStateAndRedirectsToCheck()
    {
        var inviteGuid = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel>
        {
            new() { Id = 5, Name = "Reader" },
            new() { Id = 6, Name = "Admin" }
        };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org",
            InviteGuid = inviteGuid,
            IsPending = true,
            Applications = [new ApplicationRoleChangeViewModel { OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 5, Roles = roles }]
        };
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", null, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var input = new ApplicationRoleChangePostModel
        {
            Applications = [new ApplicationRoleAssignmentPostModel { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 6 }]
        };

        var result = await _controller.ChangeInviteApplicationRolesSubmit("org", inviteGuid, input, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeInviteApplicationRolesCheck));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    [Fact]
    public async Task ChangeInviteApplicationRolesCheck_Post_WhenUpdateSucceeds_RedirectsToSuccess()
    {
        var inviteGuid = Guid.NewGuid();
        var state = new ChangeApplicationRoleState("org", null, inviteGuid, "Jane Invite", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _userService.Setup(s => s.UpdateUserApplicationRolesAsync("org", null, inviteGuid, It.IsAny<IReadOnlyList<ApplicationRoleAssignmentPostModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.ChangeInviteApplicationRolesCheckSubmit("org", inviteGuid, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeInviteApplicationRolesSuccess));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    // ─── New enrolment (granting access to a new app) ────────────────────────

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNewAccessGranted_SavesStateAndRedirectsToCheck()
    {
        var userId = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel>
        {
            new() { Id = 5, Name = "Reader" },
            new() { Id = 6, Name = "Admin" }
        };
        // App1 has no existing access
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org",
            CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel { OrganisationApplicationId = 1, ApplicationId = 10, HasExistingAccess = false, GiveAccess = false, Roles = roles }
            ]
        };
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        // User checks the box and picks a role
        var input = new ApplicationRoleChangePostModel
        {
            Applications = [new ApplicationRoleAssignmentPostModel { OrganisationApplicationId = 1, ApplicationId = 10, GiveAccess = true, SelectedRoleId = 5 }]
        };

        var result = await _controller.ChangeApplicationRolesSubmit("org", userId, input, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeApplicationRolesCheck));
        _changeApplicationRoleStateStore.Verify(s => s.SetAsync(It.Is<ChangeApplicationRoleState>(
            st => st.Applications.Any(a => !a.HasExistingAccess && a.GiveAccess && a.SelectedRoleId == 5))), Times.Once);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNewAccessGrantedWithoutRole_ReturnsViewWithRoleError()
    {
        var userId = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel> { new() { Id = 5, Name = "Reader" } };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org",
            CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel { OrganisationApplicationId = 1, HasExistingAccess = false, GiveAccess = false, Roles = roles }
            ]
        };
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        // User checks the box but does not select a role
        var input = new ApplicationRoleChangePostModel
        {
            Applications = [new ApplicationRoleAssignmentPostModel { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = null }]
        };

        var result = await _controller.ChangeApplicationRolesSubmit("org", userId, input, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("ChangeApplicationRoles");
        _controller.ModelState.ContainsKey("Applications[0].SelectedRoleId").Should().BeTrue();
        _changeApplicationRoleStateStore.Verify(s => s.SetAsync(It.IsAny<ChangeApplicationRoleState>()), Times.Never);
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Get_NewAssignment_ShowsInCheckView()
    {
        var userId = Guid.NewGuid();
        // State with a new assignment (no existing access)
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane Doe", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 10, "App1", false, true, null, string.Empty, 5, "Reader")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.ChangeApplicationRolesCheck("org", userId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("CheckApplicationRoles");
        var vm = view.Model.Should().BeOfType<ChangeApplicationRolesCheckViewModel>().Subject;
        vm.ChangedApplications.Should().ContainSingle(a =>
            a.ApplicationName == "App1" && a.NewRoleName == "Reader" && a.IsNewAssignment);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNoBoxCheckedAndNoRoleChange_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel> { new() { Id = 5, Name = "Reader" } };
        // One app with no existing access, user does not check box
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org",
            CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel { OrganisationApplicationId = 1, HasExistingAccess = false, GiveAccess = false, Roles = roles }
            ]
        };
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var input = new ApplicationRoleChangePostModel
        {
            Applications = [new ApplicationRoleAssignmentPostModel { OrganisationApplicationId = 1, GiveAccess = false }]
        };

        var result = await _controller.ChangeApplicationRolesSubmit("org", userId, input, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("ChangeApplicationRoles");
        _controller.ModelState.ContainsKey("Applications").Should().BeTrue();
    }

    [Fact]
    public async Task ChangeInviteApplicationRoles_Get_WhenInviteHasExistingAccess_ViewModelHasExistingAccessTrue()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org",
            InviteGuid = inviteGuid,
            IsPending = true,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1,
                    ApplicationClientId = "app-client-id",
                    HasExistingAccess = true,
                    GiveAccess = true,
                    SelectedRoleId = 5,
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Reader" }]
                }
            ]
        };
        _userService.Setup(s => s.GetChangeUserApplicationRolesViewModelAsync("org", null, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeInviteApplicationRoles("org", inviteGuid, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var vm = view.Model.Should().BeOfType<ChangeUserApplicationRolesViewModel>().Subject;
        vm.Applications.Should().ContainSingle(a =>
            a.HasExistingAccess && a.GiveAccess && a.SelectedRoleId == 5 && a.ApplicationClientId == "app-client-id");
    }

    [Fact]
    public async Task ChangeApplicationRolesSuccess_Get_WhenChangedApplicationsEmpty_RedirectsBackToChange()
    {
        var userId = Guid.NewGuid();
        // State where HasExistingAccess=true but role has NOT changed (CurrentRoleId == SelectedRoleId)
        // and no new access granted → ChangedApplications will be empty → redirect
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane Doe", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 5, "Reader")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.ChangeApplicationRolesSuccess("org", userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeApplicationRoles));
    }

    [Fact]
    public async Task RemoveUser_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var cdpPersonId = Guid.NewGuid();
        _userService.Setup(service => service.GetRemoveUserViewModelAsync("org", cdpPersonId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveUserViewModel?)null);

        var result = await _controller.RemoveUser("org", cdpPersonId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveUser_Get_WhenViewModelAvailable_ReturnsView()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org", UserDisplayName = "John Doe" };
        _userService.Setup(service => service.GetRemoveUserViewModelAsync("org", cdpPersonId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.RemoveUser("org", cdpPersonId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
        viewResult.ViewName.Should().Be("Remove");
    }

    [Fact]
    public async Task RemoveUser_Post_WhenNotConfirmed_RedirectsToIndex()
    {
        var cdpPersonId = Guid.NewGuid();
        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = false };

        var result = await _controller.RemoveUser("org", cdpPersonId, input, CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UsersController.Index));
        redirectResult.RouteValues.Should().Contain("organisationSlug", "org");
    }

    [Fact]
    public async Task RemoveUser_Post_WhenConfirmedAndSucceeds_RedirectsToRemoveSuccess()
    {
        var cdpPersonId = Guid.NewGuid();
        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = true };
        _userService.Setup(service => service.RemoveUserAsync("org", cdpPersonId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.RemoveUser("org", cdpPersonId, input, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.RemoveSuccess));
    }

    [Fact]
    public async Task RemoveUser_Post_WhenConfirmedButFails_ReturnsNotFound()
    {
        var cdpPersonId = Guid.NewGuid();
        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = true };
        _userService.Setup(service => service.RemoveUserAsync("org", cdpPersonId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.RemoveUser("org", cdpPersonId, input, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveInvite_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _userService.Setup(service => service.GetRemoveUserViewModelAsync("org", null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveUserViewModel?)null);

        var result = await _controller.RemoveInvite("org", 1, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveInvite_Get_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org", UserDisplayName = "John Doe" };
        _userService.Setup(service => service.GetRemoveUserViewModelAsync("org", null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.RemoveInvite("org", 1, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
        viewResult.ViewName.Should().Be("Remove");
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenNotConfirmed_RedirectsToIndex()
    {
        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = false };

        var result = await _controller.RemoveInvite("org", 1, input, CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UsersController.Index));
        redirectResult.RouteValues.Should().Contain("organisationSlug", "org");
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenConfirmedAndSucceeds_RedirectsToRemoveSuccess()
    {
        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = true };
        _userService.Setup(service => service.RemoveUserAsync("org", null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.RemoveInvite("org", 1, input, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.RemoveSuccess));
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenConfirmedButFails_ReturnsNotFound()
    {
        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = true };
        _userService.Setup(service => service.RemoveUserAsync("org", null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.RemoveInvite("org", 1, input, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveUser_Post_WhenNoOptionSelected_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetRemoveUserViewModelAsync("org", cdpPersonId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        _controller.ModelState.AddModelError(nameof(RemoveUserViewModel.RemoveConfirmed), "Select if you want to remove this user");

        var result = await _controller.RemoveUser("org", cdpPersonId, RemoveUserViewModel.Empty, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Remove");
        _userService.Verify(service => service.RemoveUserAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenNoOptionSelected_ReturnsViewWithError()
    {
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org" };
        _userService.Setup(service => service.GetRemoveUserViewModelAsync("org", null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        _controller.ModelState.AddModelError(nameof(RemoveUserViewModel.RemoveConfirmed), "Select if you want to remove this user");

        var result = await _controller.RemoveInvite("org", 1, RemoveUserViewModel.Empty, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Remove");
        _userService.Verify(service => service.RemoveUserAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
