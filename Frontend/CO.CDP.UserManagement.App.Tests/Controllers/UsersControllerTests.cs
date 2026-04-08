using CO.CDP.UserManagement.App.Controllers;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.App.Application.Users;
using CO.CDP.UserManagement.App.Application.InviteUsers;
using CO.CDP.UserManagement.App.Application.OrganisationRoles;
using CO.CDP.UserManagement.App.Application.ApplicationRoles;
using CO.CDP.UserManagement.App.Application.Removal;
using CO.CDP.UserManagement.App.Adapters;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUsersQueryService> _usersQueryService;
    private readonly Mock<IUserDetailsQueryService> _userDetailsQueryService;
    private readonly Mock<IInviteUserFlowService> _inviteUserFlowService;
    private readonly Mock<IOrganisationRoleFlowService> _organisationRoleFlowService;
    private readonly Mock<IApplicationRoleFlowService> _applicationRoleFlowService;
    private readonly Mock<IUserRemovalService> _userRemovalService;
    private readonly Mock<IOrganisationRoleService> _organisationRoleService;
    private readonly Mock<IInviteUserStateStore> _inviteUserStateStore;
    private readonly Mock<IChangeRoleStateStore> _changeRoleStateStore;
    private readonly Mock<IChangeApplicationRoleStateStore> _changeApplicationRoleStateStore;
    private readonly Mock<IUserManagementApiAdapter> _adapter;
    private readonly Mock<IInviteDetailsQueryService> _inviteDetailsQueryService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _usersQueryService = new Mock<IUsersQueryService>();
        _userDetailsQueryService = new Mock<IUserDetailsQueryService>();
        _inviteUserFlowService = new Mock<IInviteUserFlowService>();
        _organisationRoleFlowService = new Mock<IOrganisationRoleFlowService>();
        _applicationRoleFlowService = new Mock<IApplicationRoleFlowService>();
        _userRemovalService = new Mock<IUserRemovalService>();
        _organisationRoleService = new Mock<IOrganisationRoleService>();
        _inviteUserStateStore = new Mock<IInviteUserStateStore>();
        _changeRoleStateStore = new Mock<IChangeRoleStateStore>();
        _changeApplicationRoleStateStore = new Mock<IChangeApplicationRoleStateStore>();
        _adapter = new Mock<IUserManagementApiAdapter>();
        _inviteDetailsQueryService = new Mock<IInviteDetailsQueryService>();

        _inviteUserStateStore.Setup(s => s.ClearAsync()).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(s => s.ClearSuccessAsync()).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(s => s.SetAsync(It.IsAny<InviteUserState>())).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(s => s.SetSuccessAsync(It.IsAny<InviteSuccessState>())).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync((InviteUserState?)null);
        _inviteUserStateStore.Setup(s => s.GetSuccessAsync()).ReturnsAsync((InviteSuccessState?)null);
        _changeRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync((ChangeRoleState?)null);
        _changeRoleStateStore.Setup(s => s.SetAsync(It.IsAny<ChangeRoleState>())).Returns(Task.CompletedTask);
        _changeRoleStateStore.Setup(s => s.ClearAsync()).Returns(Task.CompletedTask);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync((ChangeApplicationRoleState?)null);
        _changeApplicationRoleStateStore.Setup(s => s.SetAsync(It.IsAny<ChangeApplicationRoleState>()))
            .Returns(Task.CompletedTask);
        _changeApplicationRoleStateStore.Setup(s => s.ClearAsync()).Returns(Task.CompletedTask);

        _inviteUserFlowService
            .Setup(s => s.IsEmailAlreadyInOrganisationAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRemovalService
            .Setup(s => s.IsLastOwnerAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var roles = new[]
        {
            new OrganisationRoleDefinitionResponse
                { Id = OrganisationRole.Member, DisplayName = "Member", Description = "Member description" },
            new OrganisationRoleDefinitionResponse
                { Id = OrganisationRole.Admin, DisplayName = "Admin", Description = "Admin description" },
            new OrganisationRoleDefinitionResponse
                { Id = OrganisationRole.Owner, DisplayName = "Owner", Description = "Owner description" }
        };
        _organisationRoleService.Setup(s => s.GetRolesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(roles);
        _organisationRoleService.Setup(s => s.GetRoleAsync(It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganisationRole role, CancellationToken _) => roles.FirstOrDefault(r => r.Id == role));

        _controller = new UsersController(
            _usersQueryService.Object,
            _userDetailsQueryService.Object,
            _inviteDetailsQueryService.Object,
            _inviteUserFlowService.Object,
            _organisationRoleFlowService.Object,
            _applicationRoleFlowService.Object,
            _userRemovalService.Object,
            _organisationRoleService.Object,
            _inviteUserStateStore.Object,
            _changeRoleStateStore.Object,
            _changeApplicationRoleStateStore.Object,
            _adapter.Object);
    }

    // ── Index ─────────────────────────────────────────────────────────────────

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
        _usersQueryService.Setup(s => s.GetViewModelAsync("org", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsersViewModel?)null);

        var result = await _controller.Index("org", null, null, null, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = UsersViewModel.Empty with { OrganisationName = "Org" };
        _usersQueryService.Setup(s => s.GetViewModelAsync("org", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Index("org", null, null, null, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
    }

    // ── Add (invite) ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Add_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _inviteUserFlowService.Setup(s => s.GetViewModelAsync("org", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InviteUserViewModel?)null);

        var result = await _controller.Add("org", ct: CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Add_Get_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _inviteUserFlowService.Setup(s => s.GetViewModelAsync("org", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add("org", ct: CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
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
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _inviteUserFlowService.Setup(s => s.GetViewModelAsync(
                "org",
                It.Is<InviteUserViewModel>(vm =>
                    vm.Email == state.Email &&
                    vm.FirstName == state.FirstName &&
                    vm.LastName == state.LastName &&
                    vm.OrganisationRole == state.OrganisationRole),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add("org", ct: CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Post_WhenModelStateInvalid_ReturnsView()
    {
        var input = InviteUserViewModel.Empty;
        var viewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _controller.ModelState.AddModelError("error", "invalid");
        _inviteUserFlowService.Setup(s => s.GetViewModelAsync("org", input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add("org", input, ct: CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Post_WhenValid_RedirectsToOrganisationRole()
    {
        var input = InviteUserViewModel.Empty with
        {
            Email = "user@example.com", FirstName = "First", LastName = "Last"
        };
        var result = await _controller.Add("org", input, ct: CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.OrganisationRoleStep));
    }

    [Fact]
    public async Task Add_Post_WhenReturnToCheckAnswers_RedirectsToCheckAnswers()
    {
        var input = InviteUserViewModel.Empty with
        {
            Email = "user@example.com", FirstName = "First", LastName = "Last"
        };
        var result = await _controller.Add("org", input, returnToCheckAnswers: true, ct: CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.CheckAnswersStep));
    }

    [Fact]
    public async Task Add_Post_WhenValidAndSessionExists_PreservesRoleAndAssignments()
    {
        var existingState = new InviteUserState(
            "org", "old@example.com", "Old", "Name",
            OrganisationRole.Owner,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 5 }]);
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(existingState);
        var input = InviteUserViewModel.Empty with
        {
            Email = "user@example.com", FirstName = "First", LastName = "Last"
        };

        await _controller.Add("org", input, ct: CancellationToken.None);

        _inviteUserStateStore.Verify(s => s.SetAsync(
            It.Is<InviteUserState>(st =>
                st.OrganisationRole == OrganisationRole.Owner &&
                st.ApplicationAssignments != null &&
                st.ApplicationAssignments.Count == 1 &&
                st.ApplicationAssignments[0].OrganisationApplicationId == 10 &&
                st.ApplicationAssignments[0].ApplicationRoleId == 5)), Times.Once);
    }

    [Fact]
    public async Task Add_Post_WhenEmailAlreadyInOrganisation_ReturnsViewWithError()
    {
        var input = InviteUserViewModel.Empty with
        {
            Email = "existing@example.com", FirstName = "First", LastName = "Last"
        };
        var viewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _inviteUserFlowService.Setup(s =>
                s.IsEmailAlreadyInOrganisationAsync("org", input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _inviteUserFlowService.Setup(s => s.GetViewModelAsync("org", input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "other@example.com") }))
            }
        };

        var result = await _controller.Add("org", input, ct: CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
        _controller.ModelState.ContainsKey(nameof(input.Email)).Should().BeTrue();
        _inviteUserFlowService.Verify(
            s => s.InviteAsync(It.IsAny<string>(), It.IsAny<InviteUserState>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── OrganisationRoleStep ──────────────────────────────────────────────────

    [Fact]
    public async Task OrganisationRole_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync((InviteUserState?)null);
        var result = await _controller.OrganisationRoleStep("org");
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.Add));
    }

    [Fact]
    public async Task OrganisationRole_WhenStateAvailable_ReturnsViewWithStateModel()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

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
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.OrganisationRoleStep("org", returnToCheckAnswers: true);

        result.Should().BeOfType<ViewResult>().Which.Model
            .Should().BeOfType<OrganisationRoleStepViewModel>()
            .Which.ReturnToCheckAnswers.Should().BeTrue();
    }

    // ── ApplicationRolesStep ─────────────────────────────────────────────────

    [Fact]
    public async Task ApplicationRoles_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync((InviteUserState?)null);
        var result = await _controller.ApplicationRolesStep("org", organisationRole: null, CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.Add));
    }

    [Fact]
    public async Task ApplicationRoles_WhenStateAvailable_ReturnsViewWithViewModel()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        var viewModel = new ApplicationRolesStepViewModel
        {
            OrganisationSlug = "org", FirstName = "First", LastName = "Last", Email = "user@example.com",
            Applications = []
        };
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _inviteUserFlowService.Setup(s =>
                s.GetApplicationRolesStepAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ApplicationRolesStep("org", organisationRole: null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ApplicationRoles");
        viewResult.Model.Should().BeEquivalentTo(viewModel);
    }

    [Fact]
    public async Task ApplicationRoles_Post_WhenNoSelection_ReturnsViewWithModelError()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        var viewModel = new ApplicationRolesStepViewModel
        {
            OrganisationSlug = "org", FirstName = "First", LastName = "Last", Email = "user@example.com",
            Applications =
            [
                new()
                {
                    OrganisationApplicationId = 10, ApplicationName = "Payments",
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Admin" }]
                }
            ]
        };
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _inviteUserFlowService.Setup(s =>
                s.GetApplicationRolesStepAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ApplicationRolesStepSubmit(
            "org",
            new ApplicationRolesStepPostModel
                { Applications = [new ApplicationSelectionPostModel { OrganisationApplicationId = 10 }] },
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
            OrganisationSlug = "org", FirstName = "First", LastName = "Last", Email = "user@example.com",
            Applications =
            [
                new()
                {
                    OrganisationApplicationId = 10, ApplicationName = "Payments",
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Admin" }]
                }
            ]
        };
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _inviteUserFlowService.Setup(s =>
                s.GetApplicationRolesStepAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ApplicationRolesStepSubmit(
            "org",
            new ApplicationRolesStepPostModel
            {
                Applications =
                [
                    new ApplicationSelectionPostModel
                        { OrganisationApplicationId = 10, GiveAccess = true, SelectedRoleId = 5 }
                ]
            },
            CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.CheckAnswersStep));
    }

    // ── CheckAnswers ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckAnswers_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync((InviteUserState?)null);
        var result = await _controller.CheckAnswersStep("org", null, CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.Add));
    }

    [Fact]
    public async Task CheckAnswers_WhenValidState_ReturnsView()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last", OrganisationRole.Admin,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 5 }]);
        var rolesViewModel = new ApplicationRolesStepViewModel
        {
            OrganisationSlug = "org",
            Applications =
            [
                new()
                {
                    OrganisationApplicationId = 10, ApplicationName = "Payments",
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Admin" }]
                }
            ]
        };
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _inviteUserFlowService.Setup(s =>
                s.GetApplicationRolesStepAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolesViewModel);

        var result = await _controller.CheckAnswersStep("org", null, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("CheckAnswers");
    }

    [Fact]
    public async Task CheckAnswers_Post_WhenValid_InvitesAndRedirectsToSuccess()
    {
        var state = new InviteUserState("org", "user@example.com", "First", "Last", OrganisationRole.Admin,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 5 }]);
        var rolesViewModel = new ApplicationRolesStepViewModel
        {
            OrganisationSlug = "org",
            Applications =
            [
                new()
                {
                    OrganisationApplicationId = 10, ApplicationName = "Payments",
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Admin" }]
                }
            ]
        };
        var invitePageViewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _inviteUserFlowService.Setup(s =>
                s.GetApplicationRolesStepAsync("org", state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolesViewModel);
        _inviteUserFlowService.Setup(s => s.GetViewModelAsync("org", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitePageViewModel);
        _inviteUserFlowService.Setup(s => s.InviteAsync("org", It.IsAny<InviteUserState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.CheckAnswersStepSubmit("org", CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.InviteSuccessStep));
    }

    // ── InviteSuccess ─────────────────────────────────────────────────────────

    [Fact]
    public async Task InviteSuccess_WhenStateMissing_RedirectsToIndex()
    {
        _inviteUserStateStore.Setup(s => s.GetSuccessAsync()).ReturnsAsync((InviteSuccessState?)null);
        var result = await _controller.InviteSuccessStep("org");
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.Index));
    }

    [Fact]
    public async Task InviteSuccess_WhenStateAvailable_ReturnsView()
    {
        var successState = new InviteSuccessState
        {
            OrganisationSlug = "org", OrganisationName = "Org", FirstName = "First", LastName = "Last",
            Email = "user@example.com", DateAdded = DateTimeOffset.UtcNow,
            Applications =
                [new InviteSuccessApplicationRoleViewModel { ApplicationName = "Payments", RoleName = "Admin" }]
        };
        _inviteUserStateStore.Setup(s => s.GetSuccessAsync()).ReturnsAsync(successState);

        var result = await _controller.InviteSuccessStep("org");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("InviteSuccess");
        viewResult.Model.Should().BeOfType<InviteSuccessViewModel>();
    }

    // ── ResendInvite ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ResendInvite_WhenSuccess_RedirectsToIndex()
    {
        var inviteGuid = Guid.NewGuid();
        _inviteUserFlowService.Setup(s => s.ResendInviteAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.ResendInvite("org", inviteGuid, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.Index));
    }

    [Fact]
    public async Task ResendInvite_WhenFails_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _inviteUserFlowService.Setup(s => s.ResendInviteAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));

        var result = await _controller.ResendInvite("org", inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── InviteDetails ───────────────────────────────────────────────────────

    [Fact]
    public async Task InviteDetails_WhenViewModelNull_ReturnsNotFound()
    {
        var guid = Guid.NewGuid();
        _inviteDetailsQueryService.Setup(s => s.GetViewModelAsync("org", guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InviteDetailsViewModel?)null);

        var result = await _controller.InviteDetails("org", guid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task InviteDetails_WhenViewModelAvailable_ReturnsViewWithModel()
    {
        var guid = Guid.NewGuid();
        var org = new OrganisationResponse
        {
            Id = 1,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Org",
            Slug = "org",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var vm = new InviteDetailsViewModel(org, guid, 123, "Test Invite", "invite@example.com", OrganisationRole.Member, DateTimeOffset.UtcNow, new List<string> { "AppA" });
        _inviteDetailsQueryService.Setup(s => s.GetViewModelAsync("org", guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);

        var result = await _controller.InviteDetails("org", guid, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(vm);
    }

    // ── Details ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Details_WhenViewModelNull_ReturnsNotFound()
    {
        _userDetailsQueryService.Setup(s => s.GetViewModelAsync("org", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
                ApplicationClientId: "Test-Application-Edit",
                ApplicationName: "Edit",
                ApplicationDescription: "Edit application",
                Permissions: new[] { "Read", "Write" },
                AssignedDate: DateTimeOffset.UtcNow,
                AssignedByEmail: "admin@example.com",
                ApplicationRole: "Admin"),
            new UserApplicationAccessDetailViewModel(
                ApplicationId: 2,
                ApplicationClientId: "Test-Application-View",
                ApplicationName: "View",
                ApplicationDescription: "View application",
                Permissions: new[] { "Read" },
                AssignedDate: DateTimeOffset.UtcNow.AddDays(-1),
                AssignedByEmail: "admin@example.com",
                ApplicationRole: "Editor"),
            new UserApplicationAccessDetailViewModel(
                ApplicationId: 1,
                ApplicationClientId: "Test-Application-Edit",
                ApplicationName: "Edit",
                ApplicationDescription: "Edit application",
                Permissions: new[] { "Read", "Write" },
                AssignedDate: DateTimeOffset.UtcNow,
                AssignedByEmail: "admin@example.com",
                ApplicationRole: "Admin"),
            new UserApplicationAccessDetailViewModel(
                ApplicationId: 2,
                ApplicationClientId: "Test-Application-View",
                ApplicationName: "View",
                ApplicationDescription: "View application",
                Permissions: new[] { "Read" },
                AssignedDate: DateTimeOffset.UtcNow.AddDays(-1),
                AssignedByEmail: "admin@example.com",
                ApplicationRole: "Editor")
        };
        var organisation = new OrganisationResponse
        {
            Id = 1, CdpOrganisationGuid = Guid.NewGuid(), Name = "Org", Slug = "org",
            IsActive = true, CreatedAt = DateTimeOffset.UtcNow
        };
        var viewModel = new UserDetailsViewModel(
            Organisation: organisation, CdpPersonId: Guid.NewGuid(), FullName: "Test User",
            Email: "test@example.com", OrganisationRole: OrganisationRole.Admin,
            MemberSince: "19 February 2026", ApplicationAccess: applicationAccess);
        _userDetailsQueryService.Setup(s => s.GetViewModelAsync("org", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Details("org", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
    }

    // ── ChangeRole (user) ─────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeRole_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _organisationRoleFlowService
            .Setup(s => s.GetUserViewModelAsync("org", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            OrganisationSlug = "org", CdpPersonId = userId, SelectedRole = OrganisationRole.Member
        };
        var state = new ChangeRoleState("org", userId, null, "Jane Doe", "jane@example.com", OrganisationRole.Member,
            OrganisationRole.Admin);
        _organisationRoleFlowService
            .Setup(s => s.GetUserViewModelAsync("org", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _changeRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

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
        _organisationRoleFlowService
            .Setup(s => s.GetUserViewModelAsync("org", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeRoleSubmit("org", Guid.NewGuid(), null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRole");
        viewResult.Model.Should().BeEquivalentTo(
            ChangeUserRolePageViewModel.From(viewModel,
                _organisationRoleService.Object.GetRolesAsync(CancellationToken.None).Result.ToOptions()));
    }

    [Fact]
    public async Task ChangeRoleSubmit_WhenSelfRoleChange_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationSlug = "org", CdpPersonId = userId, Email = "me@example.com",
            CurrentRole = OrganisationRole.Admin
        };
        _organisationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.ChangeRoleSubmit("org", userId, OrganisationRole.Member, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("ChangeRole");
        _controller.ModelState.ContainsKey(nameof(OrganisationRole)).Should().BeTrue();
        _organisationRoleFlowService.Verify(s => s.UpdateUserRoleAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeRole_Post_WhenSameRoleSelected_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationSlug = "org", CdpPersonId = userId, CurrentRole = OrganisationRole.Admin
        };
        _organisationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "other@example.com") })) }
        };

        var result = await _controller.ChangeRoleSubmit("org", userId, OrganisationRole.Admin, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("ChangeRole");
        _controller.ModelState.ContainsKey("organisationRole").Should().BeTrue();
    }

    [Fact]
    public async Task ChangeRole_Post_WhenRoleProvided_RedirectsToCheck()
    {
        var userId = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationSlug = "org", CdpPersonId = userId, SelectedRole = OrganisationRole.Member
        };
        _organisationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "other@example.com") })) }
        };

        var result = await _controller.ChangeRoleSubmit("org", userId, OrganisationRole.Admin, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeRoleCheck));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
        redirect.RouteValues.ContainsKey("organisationRole").Should().BeFalse();
        _organisationRoleFlowService.Verify(s => s.UpdateUserRoleAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── ChangeRoleCheck (user) ────────────────────────────────────────────────

    [Fact]
    public async Task ChangeRoleCheck_Get_WhenSessionMissing_RedirectsToChangeRole()
    {
        var result = await _controller.ChangeRoleCheck("org", Guid.NewGuid(), CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.ChangeRole));
    }

    [Fact]
    public async Task ChangeRoleCheck_Post_WhenSuccess_RedirectsToSuccess()
    {
        var userId = Guid.NewGuid();
        _changeRoleStateStore.Setup(s => s.GetAsync())
            .ReturnsAsync(new ChangeRoleState("org", userId, null, "Jane Doe", "jane@example.com",
                OrganisationRole.Member, OrganisationRole.Admin));
        _organisationRoleFlowService.Setup(s =>
                s.UpdateUserRoleAsync("org", userId, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
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
        _changeRoleStateStore.Setup(s => s.GetAsync())
            .ReturnsAsync(new ChangeRoleState("org", userId, null, "Jane Doe", "jane@example.com",
                OrganisationRole.Member, OrganisationRole.Admin));
        _organisationRoleFlowService.Setup(s =>
                s.UpdateUserRoleAsync("org", userId, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));

        var result = await _controller.ChangeRoleCheckSubmit("org", userId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeRoleCheck_Post_WhenAlreadyApplied_RedirectsToSuccess_WithoutCallingUpdate()
    {
        var userId = Guid.NewGuid();
        _changeRoleStateStore.Setup(s => s.GetAsync())
            .ReturnsAsync(new ChangeRoleState("org", userId, null, "Jane Doe", "jane@example.com",
                OrganisationRole.Member, OrganisationRole.Admin));
        _organisationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChangeUserRoleViewModel.Empty with
            {
                OrganisationSlug = "org", CdpPersonId = userId, CurrentRole = OrganisationRole.Admin
            });

        var result = await _controller.ChangeRoleCheckSubmit("org", userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeRoleSuccess));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
        _organisationRoleFlowService.Verify(s => s.UpdateUserRoleAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── ChangeRoleSuccess ─────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeRoleSuccess_Get_WhenValid_ReturnsView()
    {
        var userId = Guid.NewGuid();
        _changeRoleStateStore.Setup(s => s.GetAsync())
            .ReturnsAsync(new ChangeRoleState("org", userId, null, "Jane Doe", "jane@example.com",
                OrganisationRole.Member, OrganisationRole.Owner));

        var result = await _controller.ChangeRoleSuccess("org", userId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRoleSuccess");
        viewResult.Model.Should().BeOfType<ChangeUserRoleSuccessViewModel>().Which.NewRole.Should()
            .Be(OrganisationRole.Owner);
        _changeRoleStateStore.Verify(s => s.ClearAsync(), Times.Once);
    }

    // ── ChangeRole (invite) ───────────────────────────────────────────────────

    [Fact]
    public async Task ChangeInviteRole_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserRoleViewModel?)null);

        var result = await _controller.ChangeInviteRole("org", inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeInviteRole_Get_WhenViewModelAvailable_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org", InviteGuid = inviteGuid };
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
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
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeInviteRoleSubmit("org", inviteGuid, null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRole");
        viewResult.Model.Should().BeEquivalentTo(
            ChangeUserRolePageViewModel.From(viewModel,
                _organisationRoleService.Object.GetRolesAsync(CancellationToken.None).Result.ToOptions()));
    }

    [Fact]
    public async Task ChangeInviteRoleSubmit_WhenSelfRoleChange_ReturnsViewWithError()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationSlug = "org", InviteGuid = inviteGuid, Email = "me@example.com",
            CurrentRole = OrganisationRole.Admin
        };
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result =
            await _controller.ChangeInviteRoleSubmit("org", inviteGuid, OrganisationRole.Member,
                CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("ChangeRole");
        _controller.ModelState.ContainsKey(nameof(OrganisationRole)).Should().BeTrue();
        _organisationRoleFlowService.Verify(s => s.UpdateInviteRoleAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeInviteRole_Post_WhenSameRoleSelected_ReturnsViewWithError()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationName = "Org", InviteGuid = inviteGuid, CurrentRole = OrganisationRole.Admin
        };
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "other@example.com") })) }
        };

        var result =
            await _controller.ChangeInviteRoleSubmit("org", inviteGuid, OrganisationRole.Admin, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("ChangeRole");
        _controller.ModelState.ContainsKey("organisationRole").Should().BeTrue();
    }

    [Fact]
    public async Task ChangeInviteRole_Post_WhenRoleProvided_RedirectsToCheck()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationSlug = "org", InviteGuid = inviteGuid };
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "other@example.com") })) }
        };

        var result =
            await _controller.ChangeInviteRoleSubmit("org", inviteGuid, OrganisationRole.Admin, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeInviteRoleCheck));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    // ── ChangeInviteRoleCheck ─────────────────────────────────────────────────

    [Fact]
    public async Task ChangeInviteRoleCheck_Post_WhenSuccess_RedirectsToSuccess()
    {
        var inviteGuid = Guid.NewGuid();
        _changeRoleStateStore.Setup(s => s.GetAsync())
            .ReturnsAsync(new ChangeRoleState("org", null, inviteGuid, "Jane Invite", "jane@example.com",
                OrganisationRole.Member, OrganisationRole.Admin));
        _organisationRoleFlowService.Setup(s =>
                s.UpdateInviteRoleAsync("org", inviteGuid, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.ChangeInviteRoleCheckSubmit("org", inviteGuid, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeInviteRoleSuccess));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    [Fact]
    public async Task ChangeInviteRoleCheck_Post_WhenAlreadyApplied_RedirectsToSuccess_WithoutCallingUpdate()
    {
        var inviteGuid = Guid.NewGuid();
        _changeRoleStateStore.Setup(s => s.GetAsync())
            .ReturnsAsync(new ChangeRoleState("org", null, inviteGuid, "Jane Invite", "jane@example.com",
                OrganisationRole.Member, OrganisationRole.Admin));
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChangeUserRoleViewModel.Empty with
            {
                OrganisationSlug = "org", InviteGuid = inviteGuid, CurrentRole = OrganisationRole.Admin
            });

        var result = await _controller.ChangeInviteRoleCheckSubmit("org", inviteGuid, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeInviteRoleSuccess));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
        _organisationRoleFlowService.Verify(s => s.UpdateInviteRoleAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── ChangeInviteRoleSuccess ───────────────────────────────────────────────

    [Fact]
    public async Task ChangeInviteRoleSuccess_Get_WhenValid_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        _changeRoleStateStore.Setup(s => s.GetAsync())
            .ReturnsAsync(new ChangeRoleState("org", null, inviteGuid, "Jane Invite", "jane@example.com",
                OrganisationRole.Member, OrganisationRole.Member));

        var result = await _controller.ChangeInviteRoleSuccess("org", inviteGuid, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ChangeRoleSuccess");
        viewResult.Model.Should().BeOfType<ChangeUserRoleSuccessViewModel>().Which.NewRole.Should()
            .Be(OrganisationRole.Member);
        _changeRoleStateStore.Verify(s => s.ClearAsync(), Times.Once);
    }

    // ── ChangeApplicationRoles (user) ─────────────────────────────────────────

    [Fact]
    public async Task ChangeApplicationRoles_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _applicationRoleFlowService
            .Setup(s => s.GetUserViewModelAsync("org", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserApplicationRolesViewModel?)null);

        var result = await _controller.ChangeApplicationRoles("org", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeApplicationRoles_Get_WhenViewModelAvailable_ReturnsView()
    {
        var userId = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
            { OrganisationSlug = "org", UserDisplayName = "Jane Doe", Email = "jane@example.com" };
        _applicationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
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
            OrganisationSlug = "org", CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 10,
                    Roles =
                    [
                        new ApplicationRoleOptionViewModel { Id = 10 }, new ApplicationRoleOptionViewModel { Id = 20 }
                    ]
                }
            ]
        };
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 10, "Reader", 20, "Admin")]);
        _applicationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.ChangeApplicationRoles("org", userId, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model
            .Should().BeOfType<ChangeUserApplicationRolesViewModel>()
            .Which.Applications[0].SelectedRoleId.Should().Be(20);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNoChanges_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel> { new() { Id = 5, Name = "Reader" } };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org", CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 5,
                    Roles = roles
                }
            ]
        };
        _applicationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 5 }
            ]
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
            { new() { Id = 5, Name = "Reader" }, new() { Id = 6, Name = "Admin" } };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org", CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 5,
                    Roles = roles
                }
            ]
        };
        _applicationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 6 }
            ]
        };

        var result = await _controller.ChangeApplicationRolesSubmit("org", userId, input, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.ChangeApplicationRolesCheck));
        _changeApplicationRoleStateStore.Verify(
            s => s.SetAsync(
                It.Is<ChangeApplicationRoleState>(st => st.OrganisationSlug == "org" && st.CdpPersonId == userId)),
            Times.Once);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNewAccessGranted_SavesStateAndRedirectsToCheck()
    {
        var userId = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel>
            { new() { Id = 5, Name = "Reader" }, new() { Id = 6, Name = "Admin" } };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org", CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, ApplicationId = 10, HasExistingAccess = false, GiveAccess = false,
                    Roles = roles
                }
            ]
        };
        _applicationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, ApplicationId = 10, GiveAccess = true, SelectedRoleId = 5 }
            ]
        };

        var result = await _controller.ChangeApplicationRolesSubmit("org", userId, input, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.ChangeApplicationRolesCheck));
        _changeApplicationRoleStateStore.Verify(
            s => s.SetAsync(It.Is<ChangeApplicationRoleState>(st =>
                st.Applications.Any(a => !a.HasExistingAccess && a.GiveAccess && a.SelectedRoleId == 5))), Times.Once);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNewAccessGrantedWithoutRole_ReturnsViewWithRoleError()
    {
        var userId = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel> { new() { Id = 5, Name = "Reader" } };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org", CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                    { OrganisationApplicationId = 1, HasExistingAccess = false, GiveAccess = false, Roles = roles }
            ]
        };
        _applicationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = null }
            ]
        };

        var result = await _controller.ChangeApplicationRolesSubmit("org", userId, input, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("ChangeApplicationRoles");
        _controller.ModelState.ContainsKey("Applications[0].SelectedRoleId").Should().BeTrue();
        _changeApplicationRoleStateStore.Verify(s => s.SetAsync(It.IsAny<ChangeApplicationRoleState>()), Times.Never);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNoBoxCheckedAndNoRoleChange_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel> { new() { Id = 5, Name = "Reader" } };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org", CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                    { OrganisationApplicationId = 1, HasExistingAccess = false, GiveAccess = false, Roles = roles }
            ]
        };
        _applicationRoleFlowService.Setup(s => s.GetUserViewModelAsync("org", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
                [new ApplicationRoleAssignmentPostModel { OrganisationApplicationId = 1, GiveAccess = false }]
        };

        var result = await _controller.ChangeApplicationRolesSubmit("org", userId, input, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("ChangeApplicationRoles");
        _controller.ModelState.ContainsKey("Applications").Should().BeTrue();
    }

    // ── ChangeApplicationRolesCheck (user) ────────────────────────────────────

    [Fact]
    public async Task ChangeApplicationRolesCheck_Get_WhenNoState_RedirectsToSelection()
    {
        var result = await _controller.ChangeApplicationRolesCheck("org", Guid.NewGuid(), CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.ChangeApplicationRoles));
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
        view.Model.Should().BeOfType<ChangeApplicationRolesCheckViewModel>()
            .Which.ChangedApplications.Should().ContainSingle(a =>
                a.ApplicationName == "App1" && a.CurrentRoleName == "Reader" && a.NewRoleName == "Admin");
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Get_NewAssignment_ShowsInCheckView()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane Doe", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 10, "App1", false, true, null, string.Empty, 5, "Reader")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.ChangeApplicationRolesCheck("org", userId, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model
            .Should().BeOfType<ChangeApplicationRolesCheckViewModel>()
            .Which.ChangedApplications.Should().ContainSingle(a =>
                a.ApplicationName == "App1" && a.NewRoleName == "Reader" && a.IsNewAssignment);
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Post_WhenNoState_RedirectsToSelection()
    {
        var result = await _controller.ChangeApplicationRolesCheckSubmit("org", Guid.NewGuid(), CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.ChangeApplicationRoles));
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Post_WhenUpdateFails_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _applicationRoleFlowService.Setup(s => s.UpdateUserRolesAsync("org", userId,
                It.IsAny<IReadOnlyList<ApplicationRoleAssignmentPostModel>>(), It.IsAny<CancellationToken>()))
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
        _applicationRoleFlowService.Setup(s => s.UpdateUserRolesAsync("org", userId,
                It.IsAny<IReadOnlyList<ApplicationRoleAssignmentPostModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.ChangeApplicationRolesCheckSubmit("org", userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeApplicationRolesSuccess));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
    }

    // ── ChangeApplicationRolesSuccess ─────────────────────────────────────────

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

    [Fact]
    public async Task ChangeApplicationRolesSuccess_Get_WhenChangedApplicationsEmpty_RedirectsBackToChange()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState("org", userId, null, "Jane Doe", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 5, "Reader")]);
        _changeApplicationRoleStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.ChangeApplicationRolesSuccess("org", userId, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.ChangeApplicationRoles));
    }

    // ── ChangeApplicationRoles (invite) ───────────────────────────────────────

    [Fact]
    public async Task ChangeInviteApplicationRoles_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _applicationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserApplicationRolesViewModel?)null);

        var result = await _controller.ChangeInviteApplicationRoles("org", inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeInviteApplicationRoles_Get_WhenInviteHasExistingAccess_ViewModelHasExistingAccessTrue()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org", InviteGuid = inviteGuid, IsPending = true,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, ApplicationClientId = "app-client-id",
                    HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 5,
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Reader" }]
                }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeInviteApplicationRoles("org", inviteGuid, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model
            .Should().BeOfType<ChangeUserApplicationRolesViewModel>()
            .Which.Applications.Should().ContainSingle(a =>
                a.HasExistingAccess && a.GiveAccess && a.SelectedRoleId == 5 &&
                a.ApplicationClientId == "app-client-id");
    }

    [Fact]
    public async Task ChangeInviteApplicationRoles_Post_WhenChanged_SavesStateAndRedirectsToCheck()
    {
        var inviteGuid = Guid.NewGuid();
        var roles = new List<ApplicationRoleOptionViewModel>
            { new() { Id = 5, Name = "Reader" }, new() { Id = 6, Name = "Admin" } };
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationSlug = "org", InviteGuid = inviteGuid, IsPending = true,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 5,
                    Roles = roles
                }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync("org", inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 6 }
            ]
        };

        var result =
            await _controller.ChangeInviteApplicationRolesSubmit("org", inviteGuid, input, CancellationToken.None);

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
        _applicationRoleFlowService.Setup(s => s.UpdateInviteRolesAsync("org", inviteGuid,
                It.IsAny<IReadOnlyList<ApplicationRoleAssignmentPostModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result =
            await _controller.ChangeInviteApplicationRolesCheckSubmit("org", inviteGuid, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.ChangeInviteApplicationRolesSuccess));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    // ── RemoveUser ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveUser_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveUserViewModel?)null);

        var result = await _controller.RemoveUser("org", cdpPersonId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveUser_Get_WhenViewModelAvailable_ReturnsView()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org", UserDisplayName = "John Doe" };
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.RemoveUser("org", cdpPersonId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
        viewResult.ViewName.Should().Be("Remove");
    }

    [Fact]
    public async Task RemoveUser_Get_WhenSelfRemoval_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", Email = "me@example.com", CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.RemoveUser("org", cdpPersonId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Remove");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
        _userRemovalService.Verify(
            s => s.RemoveUserAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUser_Get_WhenLastOwner_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", Email = "owner@example.com", CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService.Setup(s => s.IsLastOwnerAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.RemoveUser("org", cdpPersonId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Remove");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
        _userRemovalService.Verify(
            s => s.RemoveUserAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUser_Post_WhenNotConfirmed_RedirectsToIndex()
    {
        var cdpPersonId = Guid.NewGuid();
        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = false };
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemoveUserViewModel.Empty with { OrganisationName = "Org" });
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.RemoveUser("org", cdpPersonId, input, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
        redirect.RouteValues.Should().Contain("organisationSlug", "org");
    }

    [Fact]
    public async Task RemoveUser_Post_WhenNoOptionSelected_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org" };
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };
        _controller.ModelState.AddModelError(nameof(RemoveUserViewModel.RemoveConfirmed),
            "Select if you want to remove this user");

        var result =
            await _controller.RemoveUser("org", cdpPersonId, RemoveUserViewModel.Empty, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("Remove");
        _userRemovalService.Verify(
            s => s.RemoveUserAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUser_Post_WhenSelfRemoval_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", Email = "me@example.com", CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.RemoveUser("org", cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("Remove");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
        _userRemovalService.Verify(
            s => s.RemoveUserAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUser_Post_WhenLastOwner_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", Email = "owner@example.com", CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService.Setup(s => s.IsLastOwnerAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.RemoveUser("org", cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("Remove");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
        _userRemovalService.Verify(
            s => s.RemoveUserAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUser_Post_ReturnsView_WhenLastOwnerCheckFails()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemoveUserViewModel.Empty with { OrganisationName = "Org" });
        _userRemovalService.Setup(s => s.IsLastOwnerAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.RemoveUser("org", cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("Remove");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
        _userRemovalService.Verify(
            s => s.RemoveUserAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUser_Post_WhenConfirmedAndSucceeds_RedirectsToRemoveSuccess()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemoveUserViewModel.Empty with { OrganisationName = "Org" });
        _userRemovalService.Setup(s => s.RemoveUserAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.RemoveUser("org", cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.RemoveSuccess));
    }

    [Fact]
    public async Task RemoveUser_Post_WhenConfirmedButFails_ReturnsNotFound()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(s => s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemoveUserViewModel.Empty with { OrganisationName = "Org" });
        _userRemovalService.Setup(s => s.RemoveUserAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.RemoveUser("org", cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── RemoveInvite ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveInvite_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _userRemovalService.Setup(s => s.GetInviteViewModelAsync("org", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveUserViewModel?)null);

        var result = await _controller.RemoveInvite("org", 1, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveInvite_Get_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org", UserDisplayName = "John Doe" };
        _userRemovalService.Setup(s => s.GetInviteViewModelAsync("org", 1, It.IsAny<CancellationToken>()))
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
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
        redirect.RouteValues.Should().Contain("organisationSlug", "org");
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenNoOptionSelected_ReturnsViewWithError()
    {
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org" };
        _userRemovalService.Setup(s => s.GetInviteViewModelAsync("org", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ModelState.AddModelError(nameof(RemoveUserViewModel.RemoveConfirmed),
            "Select if you want to remove this user");

        var result = await _controller.RemoveInvite("org", 1, RemoveUserViewModel.Empty, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("Remove");
        _userRemovalService.Verify(
            s => s.RemoveInviteAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveApplication_Get_PassesCorrectParametersToService()
    {
        var cdpPersonId = Guid.NewGuid();
        const string clientId = "test-app";
        var viewModel = new RemoveApplicationViewModel(
            OrganisationSlug: "org",
            UserDisplayName: "Jane Doe",
            UserEmail: "jane@example.com",
            ApplicationName: "Finance App",
            ApplicationSlug: clientId,
            AssignmentId: 456,
            OrgId: 1,
            UserPrincipalId: "principal-2",
            RoleName: "Editor",
            CdpPersonId: cdpPersonId);

        _userRemovalService.Setup(service => service.GetRemoveApplicationViewModelAsync("org", cdpPersonId, clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        await _controller.RemoveApplication("org", cdpPersonId, clientId, CancellationToken.None);

        _userRemovalService.Verify(service => service.GetRemoveApplicationViewModelAsync("org", cdpPersonId, clientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveApplication_Post_WhenNotConfirmed_RedirectsToDetails()
    {
        var cdpPersonId = Guid.NewGuid();
        var input = new RemoveApplicationViewModel(RevokeConfirmed: false);

        var result = await _controller.RemoveApplication("org", cdpPersonId, "test-app", input, CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UsersController.Details));
        redirectResult.RouteValues.Should().Contain("organisationSlug", "org");
        redirectResult.RouteValues.Should().Contain("cdpPersonId", cdpPersonId);
        _userRemovalService.Verify(service => service.RemoveApplicationAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveApplication_Post_WhenModelStateInvalidAndViewModelAvailable_ReturnsRemoveView()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = new RemoveApplicationViewModel(
            OrganisationSlug: "org",
            UserDisplayName: "John Doe",
            UserEmail: "john@example.com",
            ApplicationName: "Test App",
            ApplicationSlug: "test-app",
            AssignmentId: 123,
            OrgId: 1,
            UserPrincipalId: "principal-1",
            RoleName: "Admin",
            CdpPersonId: cdpPersonId);

        _userRemovalService.Setup(service => service.GetRemoveApplicationViewModelAsync("org", cdpPersonId, "test-app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ModelState.AddModelError(nameof(RemoveApplicationViewModel.RevokeConfirmed), "Select if you want to revoke access");

        var input = new RemoveApplicationViewModel(RevokeConfirmed: true);
        var result = await _controller.RemoveApplication("org", cdpPersonId, "test-app", input, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("RemoveApplication");
        viewResult.Model.Should().Be(viewModel);
        _userRemovalService.Verify(service => service.RemoveApplicationAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveApplication_Post_WhenModelStateInvalidAndViewModelMissing_ReturnsNotFound()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(service => service.GetRemoveApplicationViewModelAsync("org", cdpPersonId, "test-app", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveApplicationViewModel?)null);
        _controller.ModelState.AddModelError(nameof(RemoveApplicationViewModel.RevokeConfirmed), "Select if you want to revoke access");

        var input = new RemoveApplicationViewModel(RevokeConfirmed: true);
        var result = await _controller.RemoveApplication("org", cdpPersonId, "test-app", input, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        _userRemovalService.Verify(service => service.RemoveApplicationAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenConfirmedAndSucceeds_RedirectsToRemoveSuccess()
    {
        _userRemovalService.Setup(s => s.RemoveInviteAsync("org", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.RemoveInvite("org", 1, RemoveUserViewModel.Empty with { RemoveConfirmed = true },
            CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersController.RemoveSuccess));
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenConfirmedButFails_ReturnsNotFound()
    {
        _userRemovalService.Setup(s => s.RemoveInviteAsync("org", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "me@example.com") })) }
        };

        var result = await _controller.RemoveInvite("org", 1, RemoveUserViewModel.Empty with { RemoveConfirmed = true },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── RemoveApplicationSuccess ──────────────────────────────────────────────

    [Fact]
    public async Task RemoveApplicationSuccess_Get_WhenViewModelNull_RedirectsToIndex()
    {
        var userId = Guid.NewGuid();
        _userDetailsQueryService.Setup(s =>
                s.GetRemoveApplicationSuccessViewModelAsync("org", userId, "app-slug-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveApplicationSuccessViewModel?)null);

        var result = await _controller.RemoveApplicationSuccess("org", userId, "app-slug-1", CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersController.Index));
        redirect.RouteValues!["organisationSlug"].Should().Be("org");
    }

    [Fact]
    public async Task RemoveApplicationSuccess_Get_WhenViewModelValid_ReturnsSuccessView()
    {
        var userId = Guid.NewGuid();
        var viewModel = new RemoveApplicationSuccessViewModel
        {
            OrganisationSlug = "org", UserDisplayName = "John Doe",
            Email = "john@example.com", ApplicationName = "Test App", CdpPersonId = userId
        };
        _userDetailsQueryService.Setup(s =>
                s.GetRemoveApplicationSuccessViewModelAsync("org", userId, "app-slug-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.RemoveApplicationSuccess("org", userId, "app-slug-2", CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("RemoveApplicationSuccess");
        view.Model.Should().BeOfType<RemoveApplicationSuccessViewModel>()
            .Which.UserDisplayName.Should().Be("John Doe");
    }

    [Fact]
    public async Task RemoveApplicationSuccess_Get_PassesCorrectParametersToService()
    {
        var userId = Guid.NewGuid();
        var appId = "test-app-slug";
        var viewModel = new RemoveApplicationSuccessViewModel
        {
            OrganisationSlug = "org", UserDisplayName = "Jane Smith",
            Email = "jane@example.com", ApplicationName = "Finance App", CdpPersonId = userId
        };
        _userDetailsQueryService.Setup(s =>
                s.GetRemoveApplicationSuccessViewModelAsync("org", userId, appId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        await _controller.RemoveApplicationSuccess("org", userId, appId, CancellationToken.None);

        _userDetailsQueryService.Verify(
            s => s.GetRemoveApplicationSuccessViewModelAsync("org", userId, appId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveUser_WhenCurrentUserIsAdminAndTargetIsOwner_ReturnsViewWithPermissionError()
    {
        var cdpPersonId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var cdpClaimsJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            UserPrincipalId = "urn:fdc:test",
            Organisations = new[]
            {
                new { OrganisationId = orgId, OrganisationName = "Org", OrganisationRole = "Admin", Applications = Array.Empty<object>() }
            }
        });

        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", OrganisationSlug = "org",
            Email = "target@example.com", CurrentRole = OrganisationRole.Owner,
            CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService.Setup(s => s.IsLastOwnerAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _adapter.Setup(s => s.GetOrganisationBySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CO.CDP.UserManagement.Shared.Responses.OrganisationResponse
            {
                Id = 1, CdpOrganisationGuid = orgId, Name = "Org", Slug = "org",
                IsActive = true, CreatedAt = DateTimeOffset.UtcNow
            });

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim("email", "admin@example.com"),
                    new Claim("cdp_claims", cdpClaimsJson,
                        Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes.Json)
                }, "test"))
            }
        };

        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = true };
        var result = await _controller.RemoveUser("org", cdpPersonId, input, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Remove");
        _controller.ModelState[string.Empty]!.Errors.Should()
            .Contain(e => e.ErrorMessage.Contains("permission") || e.ErrorMessage.Contains("Owner"));
        _userRemovalService.Verify(
            s => s.RemoveUserAsync(It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUser_WhenCurrentUserIsOwnerAndTargetIsOwner_ProceedsWithRemoval()
    {
        var cdpPersonId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var cdpClaimsJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            UserPrincipalId = "urn:fdc:test",
            Organisations = new[]
            {
                new { OrganisationId = orgId, OrganisationName = "Org", OrganisationRole = "Owner", Applications = Array.Empty<object>() }
            }
        });

        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", OrganisationSlug = "org",
            Email = "target@example.com", CurrentRole = OrganisationRole.Owner,
            CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService.Setup(s => s.IsLastOwnerAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _adapter.Setup(s => s.GetOrganisationBySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CO.CDP.UserManagement.Shared.Responses.OrganisationResponse
            {
                Id = 1, CdpOrganisationGuid = orgId, Name = "Org", Slug = "org",
                IsActive = true, CreatedAt = DateTimeOffset.UtcNow
            });
        _userRemovalService.Setup(s => s.RemoveUserAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim("email", "owner@example.com"),
                    new Claim("cdp_claims", cdpClaimsJson,
                        Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes.Json)
                }, "test"))
            }
        };

        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = true };
        var result = await _controller.RemoveUser("org", cdpPersonId, input, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>();
        _userRemovalService.Verify(
            s => s.RemoveUserAsync("org", cdpPersonId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveUser_WhenCdpClaimsAbsent_DoesNotBlockRemoval()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", OrganisationSlug = "org",
            Email = "target@example.com", CurrentRole = OrganisationRole.Owner,
            CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService.Setup(s => s.IsLastOwnerAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRemovalService.Setup(s => s.RemoveUserAsync("org", cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                // No cdp_claims claim → IsAdminTargetingOwnerAsync returns false
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "someone@example.com") }))
            }
        };

        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = true };
        var result = await _controller.RemoveUser("org", cdpPersonId, input, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task RemoveSuccess_WhenViewModelIsNull_RedirectsToIndex()
    {
        var organisationSlug = "test-org";
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(s => s.GetRemoveSuccessViewModelAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveSuccessViewModel?)null);

        var result = await _controller.RemoveSuccess(organisationSlug, cdpPersonId, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(UsersController.Index));
        redirectResult.RouteValues!["organisationSlug"].Should().Be(organisationSlug);
    }

    [Fact]
    public async Task RemoveSuccess_WithValidViewModelContainingAllData_ReturnsCorrectViewModel()
    {
        var organisationSlug = "acme-corp";
        var cdpPersonId = Guid.NewGuid();
        var viewModel = new RemoveSuccessViewModel
        {
            OrganisationSlug = "acme-corp",
            UserDisplayName = "Robert Johnson",
            Email = "robert.johnson@example.com",
            OrganisationName = "ACME Corporation",
            Role = OrganisationRole.Owner,
            MemberSince = "01 March 2023",
            CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s => s.GetRemoveSuccessViewModelAsync(
                organisationSlug,
                cdpPersonId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.RemoveSuccess(organisationSlug, cdpPersonId, CancellationToken.None);

        var viewResult = (ViewResult)result;
        var returnedModel = (RemoveSuccessViewModel)viewResult.Model!;
        returnedModel.OrganisationSlug.Should().Be("acme-corp");
        returnedModel.UserDisplayName.Should().Be("Robert Johnson");
        returnedModel.Email.Should().Be("robert.johnson@example.com");
        returnedModel.OrganisationName.Should().Be("ACME Corporation");
        returnedModel.Role.Should().Be(OrganisationRole.Owner);
        returnedModel.MemberSince.Should().Be("01 March 2023");
        returnedModel.CdpPersonId.Should().Be(cdpPersonId);
    }
}