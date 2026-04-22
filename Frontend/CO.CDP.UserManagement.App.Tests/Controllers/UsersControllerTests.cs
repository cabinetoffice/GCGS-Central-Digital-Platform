using System.Text;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Application.ApplicationRoles;
using CO.CDP.UserManagement.App.Application.InviteUsers;
using CO.CDP.UserManagement.App.Application.OrganisationRoles;
using CO.CDP.UserManagement.App.Application.Removal;
using CO.CDP.UserManagement.App.Application.Users;
using CO.CDP.UserManagement.App.Controllers;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Core.Removal;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

internal static class TestOrganisationData
{
    internal static readonly Guid Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
    internal static readonly Guid OtherId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000000");
}

// ── UsersListControllerTests ──────────────────────────────────────────────────

public class UsersListControllerTests
{
    private readonly UsersListController _controller;
    private readonly Mock<IUsersQueryService> _usersQueryService = new();

    public UsersListControllerTests()
    {
        _controller = new UsersListController(_usersQueryService.Object);
    }

    [Fact]
    public async Task Index_WhenModelStateInvalid_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("error", "invalid");
        var result = await _controller.Index(TestOrganisationData.Id, null, null, null, CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Index_WhenOrganisationIdMissing_ReturnsNotFound()
    {
        var result = await _controller.Index(Guid.Empty, null, null, null, CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenViewModelNull_ReturnsNotFound()
    {
        _usersQueryService.Setup(s =>
                s.GetViewModelAsync(TestOrganisationData.Id, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsersViewModel?)null);

        var result = await _controller.Index(TestOrganisationData.Id, null, null, null, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Index_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = UsersViewModel.Empty with { OrganisationName = "Org" };
        _usersQueryService.Setup(s =>
                s.GetViewModelAsync(TestOrganisationData.Id, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Index(TestOrganisationData.Id, null, null, null, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
    }
}

// ── UserDetailsControllerTests ────────────────────────────────────────────────

public class UserDetailsControllerTests
{
    private readonly UserDetailsController _controller;
    private readonly Mock<IInviteDetailsQueryService> _inviteDetailsQueryService = new();
    private readonly Mock<IUserDetailsQueryService> _userDetailsQueryService = new();

    public UserDetailsControllerTests()
    {
        _controller = new UserDetailsController(
            _userDetailsQueryService.Object,
            _inviteDetailsQueryService.Object);
    }

    [Fact]
    public async Task Details_WhenViewModelNull_ReturnsNotFound()
    {
        _userDetailsQueryService.Setup(s =>
                s.GetViewModelAsync(TestOrganisationData.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDetailsViewModel?)null);

        var result = await _controller.Details(TestOrganisationData.Id, Guid.NewGuid(), CancellationToken.None);

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
        _userDetailsQueryService.Setup(s =>
                s.GetViewModelAsync(TestOrganisationData.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Details(TestOrganisationData.Id, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task InviteDetails_WhenViewModelNull_ReturnsNotFound()
    {
        var guid = Guid.NewGuid();
        _inviteDetailsQueryService
            .Setup(s => s.GetViewModelAsync(TestOrganisationData.Id, guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InviteDetailsViewModel?)null);

        var result = await _controller.InviteDetails(TestOrganisationData.Id, guid, CancellationToken.None);

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

        var vm = new InviteDetailsViewModel(org, guid, 123, "Test Invite", "invite@example.com",
            OrganisationRole.Member, DateTimeOffset.UtcNow, new List<string> { "AppA" });
        _inviteDetailsQueryService
            .Setup(s => s.GetViewModelAsync(TestOrganisationData.Id, guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);

        var result = await _controller.InviteDetails(TestOrganisationData.Id, guid, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(vm);
    }
}

// ── InviteUserControllerTests ─────────────────────────────────────────────────

public class InviteUserControllerTests
{
    private readonly InviteUserController _controller;
    private readonly Mock<IInviteUserFlowService> _inviteUserFlowService = new();
    private readonly Mock<IInviteUserStateStore> _inviteUserStateStore = new();
    private readonly Mock<IApplicationRoleSelectionMapper> _roleSelectionMapper = new();
    private readonly Mock<ISession> _session = new();

    public InviteUserControllerTests()
    {
        _inviteUserStateStore.Setup(s => s.ClearAsync()).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(s => s.ClearSuccessAsync()).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(s => s.SetAsync(It.IsAny<InviteUserState>())).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(s => s.SetSuccessAsync(It.IsAny<InviteSuccessState>())).Returns(Task.CompletedTask);
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync((InviteUserState?)null);
        _inviteUserStateStore.Setup(s => s.GetSuccessAsync()).ReturnsAsync((InviteSuccessState?)null);
        _inviteUserFlowService
            .Setup(s => s.IsEmailAlreadyInOrganisationAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Default: no cooldown session entry present
        var emptyBytes = Array.Empty<byte>();
        _session.Setup(s => s.TryGetValue(It.IsAny<string>(), out emptyBytes)).Returns(false);
        _session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

        var httpContext = new DefaultHttpContext { Session = _session.Object };
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        _controller = new InviteUserController(
            _inviteUserFlowService.Object,
            _inviteUserStateStore.Object,
            _roleSelectionMapper.Object,
            NullLogger<InviteUserController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = tempData
        };
    }

    [Fact]
    public async Task Add_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _inviteUserFlowService
            .Setup(s => s.GetViewModelAsync(TestOrganisationData.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InviteUserViewModel?)null);

        var result = await _controller.Add(TestOrganisationData.Id, ct: CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Add_Get_WhenViewModelAvailable_ReturnsView()
    {
        var viewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _inviteUserFlowService
            .Setup(s => s.GetViewModelAsync(TestOrganisationData.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add(TestOrganisationData.Id, ct: CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Get_WhenSessionStateExists_PrefillsFromState()
    {
        var state = new InviteUserState(TestOrganisationData.Id, "user@example.com", "First", "Last",
            OrganisationRole.Admin);
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
                TestOrganisationData.Id,
                It.Is<InviteUserViewModel>(vm =>
                    vm.Email == state.Email &&
                    vm.FirstName == state.FirstName &&
                    vm.LastName == state.LastName &&
                    vm.OrganisationRole == state.OrganisationRole),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add(TestOrganisationData.Id, ct: CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Post_WhenModelStateInvalid_ReturnsView()
    {
        var input = InviteUserViewModel.Empty;
        var viewModel = InviteUserViewModel.Empty with { OrganisationName = "Org" };
        _controller.ModelState.AddModelError("error", "invalid");
        _inviteUserFlowService
            .Setup(s => s.GetViewModelAsync(TestOrganisationData.Id, input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add(TestOrganisationData.Id, input, ct: CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Add_Post_WhenValid_RedirectsToOrganisationRole()
    {
        var input = InviteUserViewModel.Empty with
        {
            Email = "user@example.com", FirstName = "First", LastName = "Last"
        };
        var result = await _controller.Add(TestOrganisationData.Id, input, ct: CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(InviteUserController.OrganisationRoleStep));
    }

    [Fact]
    public async Task Add_Post_WhenReturnToCheckAnswers_RedirectsToCheckAnswers()
    {
        var input = InviteUserViewModel.Empty with
        {
            Email = "user@example.com", FirstName = "First", LastName = "Last"
        };
        var result = await _controller.Add(TestOrganisationData.Id, input, returnToCheckAnswers: true,
            ct: CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(InviteUserController.CheckAnswersStep));
    }

    [Fact]
    public async Task Add_Post_WhenValidAndSessionExists_PreservesRoleAndAssignments()
    {
        var existingState = new InviteUserState(
            TestOrganisationData.Id, "old@example.com", "Old", "Name",
            OrganisationRole.Owner,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 5 }]);
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(existingState);
        var input = InviteUserViewModel.Empty with
        {
            Email = "user@example.com", FirstName = "First", LastName = "Last"
        };

        await _controller.Add(TestOrganisationData.Id, input, ct: CancellationToken.None);

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
                s.IsEmailAlreadyInOrganisationAsync(TestOrganisationData.Id, input.Email,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _inviteUserFlowService
            .Setup(s => s.GetViewModelAsync(TestOrganisationData.Id, input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.Add(TestOrganisationData.Id, input, ct: CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(viewModel);
        _controller.ModelState.ContainsKey(nameof(input.Email)).Should().BeTrue();
        _inviteUserFlowService.Verify(
            s => s.InviteAsync(It.IsAny<Guid>(), It.IsAny<InviteUserState>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OrganisationRole_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync((InviteUserState?)null);
        var result = await _controller.OrganisationRoleStep(TestOrganisationData.Id);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(InviteUserController.Add));
    }

    [Fact]
    public async Task OrganisationRole_WhenStateAvailable_ReturnsViewWithStateModel()
    {
        var state = new InviteUserState(TestOrganisationData.Id, "user@example.com", "First", "Last");
        var orgRoleVm = new OrganisationRoleStepViewModel(TestOrganisationData.Id, "First", "Last", "user@example.com",
            OrganisationRole.Member, false, Array.Empty<OrganisationRoleOption>());
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _inviteUserFlowService
            .Setup(s => s.GetOrganisationRoleStepViewModelAsync(state, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgRoleVm);

        var result = await _controller.OrganisationRoleStep(TestOrganisationData.Id);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(InviteUserController.OrganisationRoleStep));
        viewResult.Model.Should().BeOfType<OrganisationRoleStepViewModel>()
            .Which.OrganisationId.Should().Be(TestOrganisationData.Id);
    }

    [Fact]
    public async Task OrganisationRole_WhenReturnToCheckAnswers_ReturnsTypedModelFlag()
    {
        var state = new InviteUserState(TestOrganisationData.Id, "user@example.com", "First", "Last");
        var orgRoleVm = new OrganisationRoleStepViewModel(TestOrganisationData.Id, "First", "Last", "user@example.com",
            OrganisationRole.Member, true, Array.Empty<OrganisationRoleOption>());
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _inviteUserFlowService
            .Setup(s => s.GetOrganisationRoleStepViewModelAsync(state, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgRoleVm);

        var result = await _controller.OrganisationRoleStep(TestOrganisationData.Id, returnToCheckAnswers: true);

        result.Should().BeOfType<ViewResult>().Which.Model
            .Should().BeOfType<OrganisationRoleStepViewModel>()
            .Which.ReturnToCheckAnswers.Should().BeTrue();
    }

    [Fact]
    public async Task ApplicationRoles_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync((InviteUserState?)null);
        var result = await _controller.ApplicationRolesStep(TestOrganisationData.Id, organisationRole: null,
            CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(InviteUserController.Add));
    }

    [Fact]
    public async Task ApplicationRoles_WhenStateAvailable_ReturnsViewWithViewModel()
    {
        var state = new InviteUserState(TestOrganisationData.Id, "user@example.com", "First", "Last");
        var viewModel = new ApplicationRolesStepViewModel
        {
            OrganisationId = TestOrganisationData.Id, FirstName = "First", LastName = "Last",
            Email = "user@example.com",
            Applications = []
        };
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);
        _inviteUserFlowService.Setup(s =>
                s.GetApplicationRolesStepAsync(TestOrganisationData.Id, state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _roleSelectionMapper.Setup(m =>
                m.ApplyExistingSelections(viewModel, It.IsAny<IReadOnlyList<InviteApplicationAssignment>?>()))
            .Returns(viewModel);

        var result = await _controller.ApplicationRolesStep(TestOrganisationData.Id, organisationRole: null,
            CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(InviteUserController.ApplicationRolesStep));
        viewResult.Model.Should().BeEquivalentTo(viewModel);
    }

    [Fact]
    public async Task ApplicationRoles_Post_WhenNoSelection_ReturnsViewWithModelError()
    {
        var state = new InviteUserState(TestOrganisationData.Id, "user@example.com", "First", "Last");
        var viewModel = new ApplicationRolesStepViewModel
        {
            OrganisationId = TestOrganisationData.Id, FirstName = "First", LastName = "Last",
            Email = "user@example.com",
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
                s.GetApplicationRolesStepAsync(TestOrganisationData.Id, state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _roleSelectionMapper.Setup(m => m.MergePostedSelections(viewModel, It.IsAny<ApplicationRolesStepPostModel>()))
            .Returns(viewModel);
        _roleSelectionMapper.Setup(m => m.ValidateSelections(viewModel, It.IsAny<ModelStateDictionary>()))
            .Returns((ApplicationRolesStepViewModel _, ModelStateDictionary ms) =>
            {
                ms.AddModelError("applicationSelections", "Please select at least one role.");
                return false;
            });

        var result = await _controller.ApplicationRolesStepSubmit(
            TestOrganisationData.Id,
            new ApplicationRolesStepPostModel
                { Applications = [new ApplicationSelectionPostModel { OrganisationApplicationId = 10 }] },
            CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(InviteUserController.ApplicationRolesStep));
        _controller.ModelState.ContainsKey("applicationSelections").Should().BeTrue();
    }

    [Fact]
    public async Task ApplicationRoles_Post_WhenValidSelection_RedirectsToCheckAnswers()
    {
        var state = new InviteUserState(TestOrganisationData.Id, "user@example.com", "First", "Last");
        var viewModel = new ApplicationRolesStepViewModel
        {
            OrganisationId = TestOrganisationData.Id, FirstName = "First", LastName = "Last",
            Email = "user@example.com",
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
                s.GetApplicationRolesStepAsync(TestOrganisationData.Id, state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _roleSelectionMapper.Setup(m => m.MergePostedSelections(viewModel, It.IsAny<ApplicationRolesStepPostModel>()))
            .Returns(viewModel);
        _roleSelectionMapper.Setup(m => m.ValidateSelections(viewModel, It.IsAny<ModelStateDictionary>()))
            .Returns(true);
        _roleSelectionMapper
            .Setup(m => m.MapToAssignments(It.IsAny<IReadOnlyList<ApplicationAccessSelectionViewModel>>()))
            .Returns([]);

        var result = await _controller.ApplicationRolesStepSubmit(
            TestOrganisationData.Id,
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
            .Be(nameof(InviteUserController.CheckAnswersStep));
    }

    [Fact]
    public async Task CheckAnswers_WhenStateMissing_RedirectsToAdd()
    {
        _inviteUserStateStore.Setup(s => s.GetAsync()).ReturnsAsync((InviteUserState?)null);
        var result = await _controller.CheckAnswersStep(TestOrganisationData.Id, null, CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(InviteUserController.Add));
    }

    [Fact]
    public async Task CheckAnswers_WhenValidState_ReturnsView()
    {
        var state = new InviteUserState(TestOrganisationData.Id, "user@example.com", "First", "Last",
            OrganisationRole.Admin,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 5 }]);
        var rolesViewModel = new ApplicationRolesStepViewModel
        {
            OrganisationId = TestOrganisationData.Id,
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
                s.GetApplicationRolesStepAsync(TestOrganisationData.Id, state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolesViewModel);

        var result = await _controller.CheckAnswersStep(TestOrganisationData.Id, null, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should()
            .Be(nameof(InviteUserController.CheckAnswersStep));
    }

    [Fact]
    public async Task CheckAnswers_Post_WhenValid_InvitesAndRedirectsToSuccess()
    {
        var state = new InviteUserState(TestOrganisationData.Id, "user@example.com", "First", "Last",
            OrganisationRole.Admin,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 5 }]);
        var rolesViewModel = new ApplicationRolesStepViewModel
        {
            OrganisationId = TestOrganisationData.Id,
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
                s.GetApplicationRolesStepAsync(TestOrganisationData.Id, state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolesViewModel);
        _inviteUserFlowService
            .Setup(s => s.GetViewModelAsync(TestOrganisationData.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitePageViewModel);
        _inviteUserFlowService.Setup(s =>
                s.InviteAsync(TestOrganisationData.Id, It.IsAny<InviteUserState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.CheckAnswersStepSubmit(TestOrganisationData.Id, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(InviteUserController.InviteSuccessStep));
    }

    [Fact]
    public async Task InviteSuccess_WhenStateMissing_RedirectsToIndex()
    {
        _inviteUserStateStore.Setup(s => s.GetSuccessAsync()).ReturnsAsync((InviteSuccessState?)null);
        var result = await _controller.InviteSuccessStep(TestOrganisationData.Id);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersListController.Index));
    }

    [Fact]
    public async Task InviteSuccess_WhenStateAvailable_ReturnsView()
    {
        var successState = new InviteSuccessState
        {
            OrganisationId = TestOrganisationData.Id, OrganisationName = "Org", FirstName = "First", LastName = "Last",
            Email = "user@example.com", DateAdded = DateTimeOffset.UtcNow,
            Applications =
                [new InviteSuccessApplicationRoleViewModel { ApplicationName = "Payments", RoleName = "Admin" }]
        };
        _inviteUserStateStore.Setup(s => s.GetSuccessAsync()).ReturnsAsync(successState);

        var result = await _controller.InviteSuccessStep(TestOrganisationData.Id);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(InviteUserController.InviteSuccessStep));
        viewResult.Model.Should().BeOfType<InviteSuccessViewModel>();
    }

    [Fact]
    public async Task ResendInvite_WhenSuccess_RedirectsToIndex()
    {
        var inviteGuid = Guid.NewGuid();
        _inviteUserFlowService.Setup(s =>
                s.ResendInviteAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<ServiceFailure, ResendInviteResult>.Success(new ResendInviteResult(ServiceOutcome.Success,
                    "Alice Smith")));

        var result = await _controller.ResendInvite(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersListController.Index));
    }

    [Fact]
    public async Task ResendInvite_WhenSuccess_SetsTempDataSuccessBanner()
    {
        var inviteGuid = Guid.NewGuid();
        _inviteUserFlowService.Setup(s =>
                s.ResendInviteAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<ServiceFailure, ResendInviteResult>.Success(new ResendInviteResult(ServiceOutcome.Success,
                    "Alice Smith")));

        await _controller.ResendInvite(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        _controller.TempData["SuccessBanner"].Should().Be("Invite resent to Alice Smith.");
    }

    [Fact]
    public async Task ResendInvite_WhenSuccessWithNoName_UsesFallback()
    {
        var inviteGuid = Guid.NewGuid();
        _inviteUserFlowService.Setup(s =>
                s.ResendInviteAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<ServiceFailure, ResendInviteResult>.Success(new ResendInviteResult(ServiceOutcome.Success)));

        await _controller.ResendInvite(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        _controller.TempData["SuccessBanner"].Should().Be("Invite resent to the user.");
    }

    [Fact]
    public async Task ResendInvite_WhenFails_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _inviteUserFlowService.Setup(s =>
                s.ResendInviteAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<ServiceFailure, ResendInviteResult>.Success(new ResendInviteResult(ServiceOutcome.NotFound)));

        var result = await _controller.ResendInvite(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ResendInvite_WhenCooldownActive_RedirectsToInviteDetailsWithInfoBanner()
    {
        var inviteGuid = Guid.NewGuid();
        var recentTimestamp = Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.AddSeconds(-10).ToString("O"));
        _session.Setup(s => s.TryGetValue($"ResendCooldown:{inviteGuid}", out recentTimestamp)).Returns(true);

        var result = await _controller.ResendInvite(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(UserDetailsController.InviteDetails));
        _controller.TempData["InfoBanner"].Should().BeOfType<string>().Which.Should().NotBeNullOrEmpty();
        _inviteUserFlowService.Verify(
            s => s.ResendInviteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResendInvite_WhenCooldownExpired_AllowsResend()
    {
        var inviteGuid = Guid.NewGuid();
        var expiredTimestamp = Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.AddMinutes(-2).ToString("O"));
        _session.Setup(s => s.TryGetValue($"ResendCooldown:{inviteGuid}", out expiredTimestamp)).Returns(true);
        _inviteUserFlowService.Setup(s =>
                s.ResendInviteAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<ServiceFailure, ResendInviteResult>.Success(new ResendInviteResult(ServiceOutcome.Success,
                    "Bob Jones")));

        var result = await _controller.ResendInvite(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersListController.Index));
    }
}

// ── ChangeOrganisationRoleControllerTests ─────────────────────────────────────

public class ChangeOrganisationRoleControllerTests
{
    private readonly ChangeOrganisationRoleController _controller;
    private readonly Mock<IOrganisationRoleFlowService> _organisationRoleFlowService = new();

    public ChangeOrganisationRoleControllerTests()
    {
        _controller = new ChangeOrganisationRoleController(_organisationRoleFlowService.Object);
    }

    [Fact]
    public async Task ChangeRole_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _organisationRoleFlowService
            .Setup(s => s.GetUserViewModelAsync(TestOrganisationData.Id, It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserRoleViewModel?)null);

        var result = await _controller.ChangeRole(TestOrganisationData.Id, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeRole_Get_WhenSessionStateExists_ReturnsViewWithStateModel()
    {
        var userId = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationId = TestOrganisationData.Id, CdpPersonId = userId, SelectedRole = OrganisationRole.Member
        };
        var state = new ChangeRoleState(TestOrganisationData.Id, userId, null, "Jane Doe", "jane@example.com",
            OrganisationRole.Member, OrganisationRole.Admin);
        var pageVm = new ChangeUserRolePageViewModel(
            OrganisationName: "Org",
            OrganisationId: TestOrganisationData.Id,
            UserDisplayName: "Jane Doe",
            Email: "jane@example.com",
            CurrentRole: OrganisationRole.Member,
            SelectedRole: OrganisationRole.Admin,
            IsPending: false,
            CdpPersonId: userId,
            InviteGuid: null,
            RoleOptions: Array.Empty<OrganisationRoleOption>());
        _organisationRoleFlowService
            .Setup(s => s.GetUserViewModelAsync(TestOrganisationData.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.GetOrCreateStateAsync(TestOrganisationData.Id, userId, null, viewModel,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _organisationRoleFlowService
            .Setup(s => s.BuildPageViewModelAsync(viewModel, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageVm);

        var result = await _controller.ChangeRole(TestOrganisationData.Id, userId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeRole));
        viewResult.Model.Should().BeOfType<ChangeUserRolePageViewModel>()
            .Which.SelectedRole.Should().Be(OrganisationRole.Admin);
    }

    [Fact]
    public async Task ChangeRole_Post_WhenRoleMissing_ReturnsView()
    {
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org" };
        var pageVm = new ChangeUserRolePageViewModel(
            "Org", TestOrganisationData.Id, "", "", OrganisationRole.Member, null, false, null, null,
            Array.Empty<OrganisationRoleOption>());
        _organisationRoleFlowService
            .Setup(s => s.GetUserViewModelAsync(TestOrganisationData.Id, It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.ValidateAndSaveRoleChangeAsync(TestOrganisationData.Id, It.IsAny<Guid?>(), null, viewModel,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationRoleChangeResult.ValidationError("organisationRole", "Select a role"));
        _organisationRoleFlowService
            .Setup(s => s.BuildPageViewModelAsync(viewModel, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageVm);

        var result =
            await _controller.ChangeRoleSubmit(TestOrganisationData.Id, Guid.NewGuid(), null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeRole));
        viewResult.Model.Should().Be(pageVm);
    }

    [Fact]
    public async Task ChangeRoleSubmit_WhenSelfRoleChange_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationId = TestOrganisationData.Id, CdpPersonId = userId, Email = "me@example.com",
            CurrentRole = OrganisationRole.Admin
        };
        var pageVm = new ChangeUserRolePageViewModel(
            "Org", TestOrganisationData.Id, "", "me@example.com", OrganisationRole.Admin, OrganisationRole.Member,
            false, userId, null,
            Array.Empty<OrganisationRoleOption>());
        _organisationRoleFlowService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.ValidateAndSaveRoleChangeAsync(TestOrganisationData.Id, (Guid?)userId, null, viewModel,
                OrganisationRole.Member,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationRoleChangeResult.ValidationError(nameof(OrganisationRole),
                "Cannot change your own role"));
        _organisationRoleFlowService
            .Setup(s => s.BuildPageViewModelAsync(viewModel, OrganisationRole.Member, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageVm);

        var result = await _controller.ChangeRoleSubmit(TestOrganisationData.Id, userId, OrganisationRole.Member,
            CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should()
            .Be(nameof(ChangeOrganisationRoleController.ChangeRole));
        _controller.ModelState.ContainsKey(nameof(OrganisationRole)).Should().BeTrue();
        _organisationRoleFlowService.Verify(s => s.UpdateUserRoleAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeRole_Post_WhenSameRoleSelected_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationId = TestOrganisationData.Id, CdpPersonId = userId, CurrentRole = OrganisationRole.Admin
        };
        var pageVm = new ChangeUserRolePageViewModel(
            "", TestOrganisationData.Id, "", "", OrganisationRole.Admin, OrganisationRole.Admin, false, userId, null,
            Array.Empty<OrganisationRoleOption>());
        _organisationRoleFlowService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.ValidateAndSaveRoleChangeAsync(TestOrganisationData.Id, (Guid?)userId, null, viewModel,
                OrganisationRole.Admin,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationRoleChangeResult.ValidationError("organisationRole", "Same role selected"));
        _organisationRoleFlowService
            .Setup(s => s.BuildPageViewModelAsync(viewModel, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageVm);

        var result = await _controller.ChangeRoleSubmit(TestOrganisationData.Id, userId, OrganisationRole.Admin,
            CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should()
            .Be(nameof(ChangeOrganisationRoleController.ChangeRole));
        _controller.ModelState.ContainsKey("organisationRole").Should().BeTrue();
    }

    [Fact]
    public async Task ChangeRole_Post_WhenRoleProvided_RedirectsToCheck()
    {
        var userId = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationId = TestOrganisationData.Id, CdpPersonId = userId, SelectedRole = OrganisationRole.Member
        };
        _organisationRoleFlowService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.ValidateAndSaveRoleChangeAsync(TestOrganisationData.Id, (Guid?)userId, null, viewModel,
                OrganisationRole.Admin,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationRoleChangeResult.Saved());

        var result = await _controller.ChangeRoleSubmit(TestOrganisationData.Id, userId, OrganisationRole.Admin,
            CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeRoleCheck));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
        redirect.RouteValues.ContainsKey("organisationRole").Should().BeFalse();
        _organisationRoleFlowService.Verify(s => s.UpdateUserRoleAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeRoleCheck_Get_WhenSessionMissing_RedirectsToChangeRole()
    {
        var result = await _controller.ChangeRoleCheck(TestOrganisationData.Id, Guid.NewGuid(), CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(ChangeOrganisationRoleController.ChangeRole));
    }

    [Fact]
    public async Task ChangeRoleCheck_Post_WhenSuccess_RedirectsToSuccess()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeRoleState(TestOrganisationData.Id, userId, null, "Jane Doe", "jane@example.com",
            OrganisationRole.Member, OrganisationRole.Admin);
        _organisationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _organisationRoleFlowService
            .Setup(s => s.GetUserViewModelAsync(TestOrganisationData.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChangeUserRoleViewModel.Empty with { CurrentRole = OrganisationRole.Member });
        _organisationRoleFlowService.Setup(s =>
                s.UpdateUserRoleAsync(TestOrganisationData.Id, userId, OrganisationRole.Admin,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result = await _controller.ChangeRoleCheckSubmit(TestOrganisationData.Id, userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeRoleSuccess));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
    }

    [Fact]
    public async Task ChangeRoleCheck_Post_WhenFails_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeRoleState(TestOrganisationData.Id, userId, null, "Jane Doe", "jane@example.com",
            OrganisationRole.Member, OrganisationRole.Admin);
        _organisationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _organisationRoleFlowService
            .Setup(s => s.GetUserViewModelAsync(TestOrganisationData.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChangeUserRoleViewModel.Empty with { CurrentRole = OrganisationRole.Member });
        _organisationRoleFlowService.Setup(s =>
                s.UpdateUserRoleAsync(TestOrganisationData.Id, userId, OrganisationRole.Admin,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));

        var result = await _controller.ChangeRoleCheckSubmit(TestOrganisationData.Id, userId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeRoleCheck_Post_WhenAlreadyApplied_RedirectsToSuccess_WithoutCallingUpdate()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeRoleState(TestOrganisationData.Id, userId, null, "Jane Doe", "jane@example.com",
            OrganisationRole.Member, OrganisationRole.Admin);
        _organisationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _organisationRoleFlowService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChangeUserRoleViewModel.Empty with
            {
                OrganisationId = TestOrganisationData.Id, CdpPersonId = userId, CurrentRole = OrganisationRole.Admin
            });

        var result = await _controller.ChangeRoleCheckSubmit(TestOrganisationData.Id, userId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeRoleSuccess));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
        _organisationRoleFlowService.Verify(s => s.UpdateUserRoleAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeRoleSuccess_Get_WhenValid_ReturnsView()
    {
        var userId = Guid.NewGuid();
        var successVm = new ChangeUserRoleSuccessViewModel(TestOrganisationData.Id, "Jane Doe", OrganisationRole.Owner,
            "Owner description");
        _organisationRoleFlowService
            .Setup(s => s.GetSuccessViewModelAsync(TestOrganisationData.Id, userId, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successVm);

        var result = await _controller.ChangeRoleSuccess(TestOrganisationData.Id, userId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeRoleSuccess));
        viewResult.Model.Should().BeOfType<ChangeUserRoleSuccessViewModel>().Which.NewRole.Should()
            .Be(OrganisationRole.Owner);
    }

    [Fact]
    public async Task ChangeInviteRole_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserRoleViewModel?)null);

        var result = await _controller.ChangeInviteRole(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeInviteRole_Get_WhenViewModelAvailable_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org", InviteGuid = inviteGuid };
        var state = new ChangeRoleState(TestOrganisationData.Id, null, inviteGuid, "Jane Invite", "jane@example.com",
            OrganisationRole.Member, OrganisationRole.Member);
        var pageVm = new ChangeUserRolePageViewModel(
            OrganisationName: "Org",
            OrganisationId: TestOrganisationData.Id,
            UserDisplayName: "Jane Invite",
            Email: "jane@example.com",
            CurrentRole: OrganisationRole.Member,
            SelectedRole: OrganisationRole.Member,
            IsPending: true,
            CdpPersonId: null,
            InviteGuid: inviteGuid,
            RoleOptions: Array.Empty<OrganisationRoleOption>());
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.GetOrCreateStateAsync(TestOrganisationData.Id, null, inviteGuid, viewModel,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _organisationRoleFlowService
            .Setup(s => s.BuildPageViewModelAsync(viewModel, OrganisationRole.Member, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageVm);

        var result = await _controller.ChangeInviteRole(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeRole));
        viewResult.Model.Should().BeOfType<ChangeUserRolePageViewModel>()
            .Which.SelectedRole.Should().Be(OrganisationRole.Member);
    }

    [Fact]
    public async Task ChangeInviteRole_Post_WhenRoleMissing_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with { OrganisationName = "Org", InviteGuid = inviteGuid };
        var pageVm = new ChangeUserRolePageViewModel(
            "Org", TestOrganisationData.Id, "", "", OrganisationRole.Member, null, false, null, inviteGuid,
            Array.Empty<OrganisationRoleOption>());
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.ValidateAndSaveRoleChangeAsync(TestOrganisationData.Id, null, inviteGuid, viewModel, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationRoleChangeResult.ValidationError("organisationRole", "Select a role"));
        _organisationRoleFlowService
            .Setup(s => s.BuildPageViewModelAsync(viewModel, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageVm);

        var result =
            await _controller.ChangeInviteRoleSubmit(TestOrganisationData.Id, inviteGuid, null, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeRole));
        viewResult.Model.Should().Be(pageVm);
    }

    [Fact]
    public async Task ChangeInviteRoleSubmit_WhenSelfRoleChange_ReturnsViewWithError()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationId = TestOrganisationData.Id, InviteGuid = inviteGuid, Email = "me@example.com",
            CurrentRole = OrganisationRole.Admin
        };
        var pageVm = new ChangeUserRolePageViewModel(
            "", TestOrganisationData.Id, "", "me@example.com", OrganisationRole.Admin, OrganisationRole.Member, true,
            null, inviteGuid,
            Array.Empty<OrganisationRoleOption>());
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.ValidateAndSaveRoleChangeAsync(TestOrganisationData.Id, null, inviteGuid, viewModel,
                OrganisationRole.Member,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationRoleChangeResult.ValidationError(nameof(OrganisationRole),
                "Cannot change your own role"));
        _organisationRoleFlowService
            .Setup(s => s.BuildPageViewModelAsync(viewModel, OrganisationRole.Member, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageVm);

        var result =
            await _controller.ChangeInviteRoleSubmit(TestOrganisationData.Id, inviteGuid, OrganisationRole.Member,
                CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should()
            .Be(nameof(ChangeOrganisationRoleController.ChangeRole));
        _controller.ModelState.ContainsKey(nameof(OrganisationRole)).Should().BeTrue();
        _organisationRoleFlowService.Verify(s => s.UpdateInviteRoleAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
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
        var pageVm = new ChangeUserRolePageViewModel(
            "Org", TestOrganisationData.Id, "", "", OrganisationRole.Admin, OrganisationRole.Admin, true, null,
            inviteGuid,
            Array.Empty<OrganisationRoleOption>());
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.ValidateAndSaveRoleChangeAsync(TestOrganisationData.Id, null, inviteGuid, viewModel,
                OrganisationRole.Admin,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationRoleChangeResult.ValidationError("organisationRole", "Same role selected"));
        _organisationRoleFlowService
            .Setup(s => s.BuildPageViewModelAsync(viewModel, OrganisationRole.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageVm);

        var result =
            await _controller.ChangeInviteRoleSubmit(TestOrganisationData.Id, inviteGuid, OrganisationRole.Admin,
                CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should()
            .Be(nameof(ChangeOrganisationRoleController.ChangeRole));
        _controller.ModelState.ContainsKey("organisationRole").Should().BeTrue();
    }

    [Fact]
    public async Task ChangeInviteRole_Post_WhenRoleProvided_RedirectsToCheck()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = ChangeUserRoleViewModel.Empty with
        {
            OrganisationId = TestOrganisationData.Id, InviteGuid = inviteGuid
        };
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _organisationRoleFlowService
            .Setup(s => s.ValidateAndSaveRoleChangeAsync(TestOrganisationData.Id, null, inviteGuid, viewModel,
                OrganisationRole.Admin,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationRoleChangeResult.Saved());

        var result =
            await _controller.ChangeInviteRoleSubmit(TestOrganisationData.Id, inviteGuid, OrganisationRole.Admin,
                CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeInviteRoleCheck));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    [Fact]
    public async Task ChangeInviteRoleCheck_Post_WhenSuccess_RedirectsToSuccess()
    {
        var inviteGuid = Guid.NewGuid();
        var state = new ChangeRoleState(TestOrganisationData.Id, null, inviteGuid, "Jane Invite", "jane@example.com",
            OrganisationRole.Member, OrganisationRole.Admin);
        _organisationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, null, inviteGuid,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChangeUserRoleViewModel.Empty with { CurrentRole = OrganisationRole.Member });
        _organisationRoleFlowService.Setup(s =>
                s.UpdateInviteRoleAsync(TestOrganisationData.Id, inviteGuid, OrganisationRole.Admin,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result =
            await _controller.ChangeInviteRoleCheckSubmit(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeInviteRoleSuccess));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    [Fact]
    public async Task ChangeInviteRoleCheck_Post_WhenAlreadyApplied_RedirectsToSuccess_WithoutCallingUpdate()
    {
        var inviteGuid = Guid.NewGuid();
        var state = new ChangeRoleState(TestOrganisationData.Id, null, inviteGuid, "Jane Invite", "jane@example.com",
            OrganisationRole.Member, OrganisationRole.Admin);
        _organisationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, null, inviteGuid,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _organisationRoleFlowService
            .Setup(s => s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChangeUserRoleViewModel.Empty with
            {
                OrganisationId = TestOrganisationData.Id, InviteGuid = inviteGuid, CurrentRole = OrganisationRole.Admin
            });

        var result =
            await _controller.ChangeInviteRoleCheckSubmit(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeInviteRoleSuccess));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
        _organisationRoleFlowService.Verify(s => s.UpdateInviteRoleAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<OrganisationRole>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeInviteRoleSuccess_Get_WhenValid_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        var successVm = new ChangeUserRoleSuccessViewModel(TestOrganisationData.Id, "Jane Invite",
            OrganisationRole.Member, "Member description");
        _organisationRoleFlowService
            .Setup(s => s.GetSuccessViewModelAsync(TestOrganisationData.Id, null, inviteGuid,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successVm);

        var result =
            await _controller.ChangeInviteRoleSuccess(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(ChangeOrganisationRoleController.ChangeRoleSuccess));
        viewResult.Model.Should().BeOfType<ChangeUserRoleSuccessViewModel>().Which.NewRole.Should()
            .Be(OrganisationRole.Member);
    }
}

// ── ChangeApplicationRolesControllerTests ─────────────────────────────────────

public class ChangeApplicationRolesControllerTests
{
    private readonly Mock<IApplicationRoleFlowService> _applicationRoleFlowService = new();
    private readonly ChangeApplicationRolesController _controller;

    public ChangeApplicationRolesControllerTests()
    {
        _controller = new ChangeApplicationRolesController(_applicationRoleFlowService.Object);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Get_WhenViewModelNull_ReturnsNotFound()
    {
        _applicationRoleFlowService
            .Setup(s => s.GetUserViewModelWithStateAsync(TestOrganisationData.Id, It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserApplicationRolesViewModel?)null);

        var result =
            await _controller.ChangeApplicationRoles(TestOrganisationData.Id, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeApplicationRoles_Get_WhenViewModelAvailable_ReturnsView()
    {
        var userId = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
            { OrganisationId = TestOrganisationData.Id, UserDisplayName = "Jane Doe", Email = "jane@example.com" };
        _applicationRoleFlowService
            .Setup(s => s.GetUserViewModelWithStateAsync(TestOrganisationData.Id, userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeApplicationRoles(TestOrganisationData.Id, userId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be(nameof(ChangeApplicationRolesController.ChangeApplicationRoles));
        view.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Get_RestoresSelectionsFromSessionState()
    {
        var userId = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationId = TestOrganisationData.Id, CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 20,
                    Roles =
                    [
                        new ApplicationRoleOptionViewModel { Id = 10 }, new ApplicationRoleOptionViewModel { Id = 20 }
                    ]
                }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.GetUserViewModelWithStateAsync(TestOrganisationData.Id, userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.ChangeApplicationRoles(TestOrganisationData.Id, userId, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model
            .Should().BeOfType<ChangeUserApplicationRolesViewModel>()
            .Which.Applications[0].SelectedRoleId.Should().Be(20);
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNoChanges_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationId = TestOrganisationData.Id, CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, HasExistingAccess = true, GiveAccess = true, SelectedRoleId = 5,
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Reader" }]
                }
            ]
        };
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 5 }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.ProcessSubmitAsync(TestOrganisationData.Id, userId, null, input,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ApplicationRoleSubmitResult.ValidationError(viewModel, [("Applications", "No changes detected")]));

        var result =
            await _controller.ChangeApplicationRolesSubmit(TestOrganisationData.Id, userId, input,
                CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be(nameof(ChangeApplicationRolesController.ChangeApplicationRoles));
        _controller.ModelState.ContainsKey("Applications").Should().BeTrue();
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenChanged_SavesStateAndRedirectsToCheck()
    {
        var userId = Guid.NewGuid();
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 6 }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.ProcessSubmitAsync(TestOrganisationData.Id, userId, null, input,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRoleSubmitResult.Saved());

        var result =
            await _controller.ChangeApplicationRolesSubmit(TestOrganisationData.Id, userId, input,
                CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(ChangeApplicationRolesController.ChangeApplicationRolesCheck));
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNewAccessGranted_SavesStateAndRedirectsToCheck()
    {
        var userId = Guid.NewGuid();
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, ApplicationId = 10, GiveAccess = true, SelectedRoleId = 5 }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.ProcessSubmitAsync(TestOrganisationData.Id, userId, null, input,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRoleSubmitResult.Saved());

        var result =
            await _controller.ChangeApplicationRolesSubmit(TestOrganisationData.Id, userId, input,
                CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(ChangeApplicationRolesController.ChangeApplicationRolesCheck));
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNewAccessGrantedWithoutRole_ReturnsViewWithRoleError()
    {
        var userId = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationId = TestOrganisationData.Id, CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, HasExistingAccess = false, GiveAccess = false,
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Reader" }]
                }
            ]
        };
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = null }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.ProcessSubmitAsync(TestOrganisationData.Id, userId, null, input,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRoleSubmitResult.ValidationError(viewModel,
                [("Applications[0].SelectedRoleId", "Select a role")]));

        var result =
            await _controller.ChangeApplicationRolesSubmit(TestOrganisationData.Id, userId, input,
                CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should()
            .Be(nameof(ChangeApplicationRolesController.ChangeApplicationRoles));
        _controller.ModelState.ContainsKey("Applications[0].SelectedRoleId").Should().BeTrue();
    }

    [Fact]
    public async Task ChangeApplicationRoles_Post_WhenNoBoxCheckedAndNoRoleChange_ReturnsViewWithError()
    {
        var userId = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationId = TestOrganisationData.Id, CdpPersonId = userId,
            Applications =
            [
                new ApplicationRoleChangeViewModel
                {
                    OrganisationApplicationId = 1, HasExistingAccess = false, GiveAccess = false,
                    Roles = [new ApplicationRoleOptionViewModel { Id = 5, Name = "Reader" }]
                }
            ]
        };
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
                [new ApplicationRoleAssignmentPostModel { OrganisationApplicationId = 1, GiveAccess = false }]
        };
        _applicationRoleFlowService
            .Setup(s => s.ProcessSubmitAsync(TestOrganisationData.Id, userId, null, input,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRoleSubmitResult.ValidationError(viewModel,
                [("Applications", "No changes detected")]));

        var result =
            await _controller.ChangeApplicationRolesSubmit(TestOrganisationData.Id, userId, input,
                CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should()
            .Be(nameof(ChangeApplicationRolesController.ChangeApplicationRoles));
        _controller.ModelState.ContainsKey("Applications").Should().BeTrue();
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Get_WhenNoState_RedirectsToSelection()
    {
        var result =
            await _controller.ChangeApplicationRolesCheck(TestOrganisationData.Id, Guid.NewGuid(),
                CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(ChangeApplicationRolesController.ChangeApplicationRoles));
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Get_WhenStateValid_ReturnsCheckView()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState(TestOrganisationData.Id, userId, null, "Jane Doe",
            "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        var checkVm = new ChangeApplicationRolesCheckViewModel
        {
            ChangedApplications =
            [
                new ChangedApplicationRoleViewModel
                    { ApplicationName = "App1", CurrentRoleName = "Reader", NewRoleName = "Admin" }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _applicationRoleFlowService.Setup(s => s.BuildCheckViewModel(state)).Returns(checkVm);

        var result =
            await _controller.ChangeApplicationRolesCheck(TestOrganisationData.Id, userId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be(nameof(ChangeApplicationRolesController.ChangeApplicationRolesCheck));
        view.Model.Should().BeOfType<ChangeApplicationRolesCheckViewModel>()
            .Which.ChangedApplications.Should().ContainSingle(a =>
                a.ApplicationName == "App1" && a.CurrentRoleName == "Reader" && a.NewRoleName == "Admin");
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Get_NewAssignment_ShowsInCheckView()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState(TestOrganisationData.Id, userId, null, "Jane Doe",
            "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 10, "App1", false, true, null, string.Empty, 5, "Reader")]);
        var checkVm = new ChangeApplicationRolesCheckViewModel
        {
            ChangedApplications =
            [
                new ChangedApplicationRoleViewModel
                    { ApplicationName = "App1", NewRoleName = "Reader", IsNewAssignment = true }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _applicationRoleFlowService.Setup(s => s.BuildCheckViewModel(state)).Returns(checkVm);

        var result =
            await _controller.ChangeApplicationRolesCheck(TestOrganisationData.Id, userId, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.Model
            .Should().BeOfType<ChangeApplicationRolesCheckViewModel>()
            .Which.ChangedApplications.Should().ContainSingle(a =>
                a.ApplicationName == "App1" && a.NewRoleName == "Reader" && a.IsNewAssignment);
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Post_WhenNoState_RedirectsToSelection()
    {
        var result =
            await _controller.ChangeApplicationRolesCheckSubmit(TestOrganisationData.Id, Guid.NewGuid(),
                CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(ChangeApplicationRolesController.ChangeApplicationRoles));
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Post_WhenUpdateFails_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState(TestOrganisationData.Id, userId, null, "Jane", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        _applicationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _applicationRoleFlowService.Setup(s => s.BuildAssignments(state))
            .Returns(new List<ApplicationRoleAssignmentPostModel>());
        _applicationRoleFlowService.Setup(s => s.UpdateUserRolesAsync(TestOrganisationData.Id, userId,
                It.IsAny<IReadOnlyList<ApplicationRoleAssignmentPostModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));

        var result =
            await _controller.ChangeApplicationRolesCheckSubmit(TestOrganisationData.Id, userId,
                CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeApplicationRolesCheck_Post_WhenUpdateSucceeds_RedirectsToSuccess()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState(TestOrganisationData.Id, userId, null, "Jane", "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        _applicationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _applicationRoleFlowService.Setup(s => s.BuildAssignments(state))
            .Returns(new List<ApplicationRoleAssignmentPostModel>());
        _applicationRoleFlowService.Setup(s => s.UpdateUserRolesAsync(TestOrganisationData.Id, userId,
                It.IsAny<IReadOnlyList<ApplicationRoleAssignmentPostModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result =
            await _controller.ChangeApplicationRolesCheckSubmit(TestOrganisationData.Id, userId,
                CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ChangeApplicationRolesController.ChangeApplicationRolesSuccess));
        redirect.RouteValues!["cdpPersonId"].Should().Be(userId);
    }

    [Fact]
    public async Task ChangeApplicationRolesSuccess_Get_WhenStateValid_ReturnsSuccessViewAndClearsState()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState(TestOrganisationData.Id, userId, null, "Jane Doe",
            "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        var successVm = new ChangeApplicationRolesSuccessViewModel { UserDisplayName = "Jane Doe" };
        _applicationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _applicationRoleFlowService.Setup(s => s.ClearStateAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _applicationRoleFlowService.Setup(s => s.BuildSuccessViewModel(TestOrganisationData.Id, state))
            .Returns(successVm);

        var result =
            await _controller.ChangeApplicationRolesSuccess(TestOrganisationData.Id, userId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be(nameof(ChangeApplicationRolesController.ChangeApplicationRolesSuccess));
        view.Model.Should().BeOfType<ChangeApplicationRolesSuccessViewModel>()
            .Which.UserDisplayName.Should().Be("Jane Doe");
        _applicationRoleFlowService.Verify(s => s.ClearStateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeApplicationRolesSuccess_Get_WhenChangedApplicationsEmpty_RedirectsBackToChange()
    {
        var userId = Guid.NewGuid();
        var state = new ChangeApplicationRoleState(TestOrganisationData.Id, userId, null, "Jane Doe",
            "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 5, "Reader")]);
        _applicationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _applicationRoleFlowService.Setup(s => s.ClearStateAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _applicationRoleFlowService.Setup(s => s.BuildSuccessViewModel(TestOrganisationData.Id, state))
            .Returns((ChangeApplicationRolesSuccessViewModel?)null);

        var result =
            await _controller.ChangeApplicationRolesSuccess(TestOrganisationData.Id, userId, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(ChangeApplicationRolesController.ChangeApplicationRoles));
    }

    [Fact]
    public async Task ChangeInviteApplicationRoles_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _applicationRoleFlowService
            .Setup(s => s.GetInviteViewModelWithStateAsync(TestOrganisationData.Id, inviteGuid,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChangeUserApplicationRolesViewModel?)null);

        var result =
            await _controller.ChangeInviteApplicationRoles(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ChangeInviteApplicationRoles_Get_WhenInviteHasExistingAccess_ViewModelHasExistingAccessTrue()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = new ChangeUserApplicationRolesViewModel
        {
            OrganisationId = TestOrganisationData.Id, InviteGuid = inviteGuid, IsPending = true,
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
            .Setup(s => s.GetInviteViewModelWithStateAsync(TestOrganisationData.Id, inviteGuid,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result =
            await _controller.ChangeInviteApplicationRoles(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

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
        var input = new ApplicationRoleChangePostModel
        {
            Applications =
            [
                new ApplicationRoleAssignmentPostModel
                    { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 6 }
            ]
        };
        _applicationRoleFlowService
            .Setup(s => s.ProcessSubmitAsync(TestOrganisationData.Id, null, inviteGuid, input,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRoleSubmitResult.Saved());

        var result =
            await _controller.ChangeInviteApplicationRolesSubmit(TestOrganisationData.Id, inviteGuid, input,
                CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ChangeApplicationRolesController.ChangeInviteApplicationRolesCheck));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }

    [Fact]
    public async Task ChangeInviteApplicationRolesCheck_Post_WhenUpdateSucceeds_RedirectsToSuccess()
    {
        var inviteGuid = Guid.NewGuid();
        var state = new ChangeApplicationRoleState(TestOrganisationData.Id, null, inviteGuid, "Jane Invite",
            "jane@example.com",
            [new ApplicationRoleAssignmentState(1, 0, "App1", true, true, 5, "Reader", 6, "Admin")]);
        _applicationRoleFlowService
            .Setup(s => s.GetValidatedStateAsync(TestOrganisationData.Id, null, inviteGuid,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _applicationRoleFlowService.Setup(s => s.BuildAssignments(state))
            .Returns(new List<ApplicationRoleAssignmentPostModel>());
        _applicationRoleFlowService.Setup(s => s.UpdateInviteRolesAsync(TestOrganisationData.Id, inviteGuid,
                It.IsAny<IReadOnlyList<ApplicationRoleAssignmentPostModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result =
            await _controller.ChangeInviteApplicationRolesCheckSubmit(TestOrganisationData.Id, inviteGuid,
                CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ChangeApplicationRolesController.ChangeInviteApplicationRolesSuccess));
        redirect.RouteValues!["inviteGuid"].Should().Be(inviteGuid);
    }
}

// ── RemovalControllerTests ────────────────────────────────────────────────────

public class RemovalControllerTests
{
    private readonly RemovalController _controller;
    private readonly Mock<IRemoveInviteStateStore> _removeInviteStateStore = new();
    private readonly Mock<IUserDetailsQueryService> _userDetailsQueryService = new();
    private readonly Mock<IUserRemovalService> _userRemovalService = new();

    public RemovalControllerTests()
    {
        _userRemovalService
            .Setup(s => s.ValidateRemovalAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemovalValidationResult.Success());
        _removeInviteStateStore.Setup(s => s.GetAsync()).ReturnsAsync((RemoveInviteSuccessState?)null);
        _removeInviteStateStore.Setup(s => s.SetAsync(It.IsAny<RemoveInviteSuccessState>()))
            .Returns(Task.CompletedTask);
        _removeInviteStateStore.Setup(s => s.ClearAsync()).Returns(Task.CompletedTask);

        _controller = new RemovalController(
            _userRemovalService.Object,
            _userDetailsQueryService.Object,
            _removeInviteStateStore.Object);
    }

    [Fact]
    public async Task RemoveUser_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveUserViewModel?)null);

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveUser_Get_WhenViewModelAvailable_ReturnsView()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org", UserDisplayName = "John Doe" };
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
        viewResult.ViewName.Should().Be("RemoveUser");
    }

    [Fact]
    public async Task RemoveUser_Get_WhenSelfRemoval_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", Email = "me@example.com", CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService.Setup(s =>
                s.ValidateRemovalAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemovalValidationResult.Fail("You cannot remove yourself from the organisation."));

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("RemoveUser");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUser_Get_WhenLastOwner_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", Email = "owner@example.com", CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService.Setup(s =>
                s.ValidateRemovalAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemovalValidationResult.Fail("You cannot remove the last owner of the organisation."));

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("RemoveUser");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUser_Post_WhenNotConfirmed_RedirectsToIndex()
    {
        var cdpPersonId = Guid.NewGuid();
        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = false };
        _userRemovalService
            .Setup(s => s.ValidateAndRemoveUserAsync(TestOrganisationData.Id, cdpPersonId, false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRemovalSubmitResult.Cancelled());

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId, input, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersListController.Index));
        redirect.RouteValues.Should().Contain("id", TestOrganisationData.Id);
    }

    [Fact]
    public async Task RemoveUser_Post_WhenNoOptionSelected_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org" };
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService
            .Setup(s => s.ValidateAndRemoveUserAsync(TestOrganisationData.Id, cdpPersonId, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRemovalSubmitResult.ValidationError("Select yes or no"));

        var result =
            await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId, RemoveUserViewModel.Empty,
                CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("RemoveUser");
    }

    [Fact]
    public async Task RemoveUser_Post_WhenSelfRemoval_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", Email = "me@example.com", CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService
            .Setup(s => s.ValidateAndRemoveUserAsync(TestOrganisationData.Id, cdpPersonId, true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRemovalSubmitResult.ValidationError("Cannot remove yourself"));

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("RemoveUser");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUser_Post_WhenLastOwner_ReturnsViewWithError()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with
        {
            OrganisationName = "Org", Email = "owner@example.com", CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _userRemovalService
            .Setup(s => s.ValidateAndRemoveUserAsync(TestOrganisationData.Id, cdpPersonId, true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRemovalSubmitResult.ValidationError("Cannot remove last owner"));

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("RemoveUser");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUser_Post_ReturnsView_WhenLastOwnerCheckFails()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(s =>
                s.GetUserViewModelAsync(TestOrganisationData.Id, cdpPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemoveUserViewModel.Empty with { OrganisationName = "Org" });
        _userRemovalService
            .Setup(s => s.ValidateAndRemoveUserAsync(TestOrganisationData.Id, cdpPersonId, true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRemovalSubmitResult.ValidationError("error"));

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("RemoveUser");
        _controller.ModelState.ContainsKey(string.Empty).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUser_Post_WhenConfirmedAndSucceeds_RedirectsToRemoveSuccess()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService
            .Setup(s => s.ValidateAndRemoveUserAsync(TestOrganisationData.Id, cdpPersonId, true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRemovalSubmitResult.Removed());

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(RemovalController.RemoveSuccess));
    }

    [Fact]
    public async Task RemoveUser_Post_WhenConfirmedButFails_ReturnsNotFound()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService
            .Setup(s => s.ValidateAndRemoveUserAsync(TestOrganisationData.Id, cdpPersonId, true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRemovalSubmitResult.NotFound());

        var result = await _controller.RemoveUser(TestOrganisationData.Id, cdpPersonId,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true }, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveInvite_Get_WhenViewModelNull_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _userRemovalService.Setup(s =>
                s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveUserViewModel?)null);

        var result = await _controller.RemoveInvite(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveInvite_Get_WhenViewModelAvailable_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org", UserDisplayName = "John Doe" };
        _userRemovalService.Setup(s =>
                s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.RemoveInvite(TestOrganisationData.Id, inviteGuid, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(viewModel);
        viewResult.ViewName.Should().Be("RemoveUser");
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenNotConfirmed_RedirectsToIndex()
    {
        var inviteGuid = Guid.NewGuid();
        _userRemovalService.Setup(s =>
                s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemoveUserViewModel.Empty);
        var input = RemoveUserViewModel.Empty with { RemoveConfirmed = false };
        var result = await _controller.RemoveInvite(TestOrganisationData.Id, inviteGuid, input, CancellationToken.None);
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersListController.Index));
        redirect.RouteValues.Should().Contain("id", TestOrganisationData.Id);
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenNoOptionSelected_ReturnsViewWithError()
    {
        var inviteGuid = Guid.NewGuid();
        var viewModel = RemoveUserViewModel.Empty with { OrganisationName = "Org" };
        _userRemovalService.Setup(s =>
                s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ModelState.AddModelError(nameof(RemoveUserViewModel.RemoveConfirmed),
            "Select if you want to remove this user");

        var result = await _controller.RemoveInvite(TestOrganisationData.Id, inviteGuid, RemoveUserViewModel.Empty,
            CancellationToken.None);

        result.Should().BeOfType<ViewResult>().Which.ViewName.Should().Be("RemoveUser");
        _userRemovalService.Verify(
            s => s.RemoveInviteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenConfirmedAndSucceeds_RedirectsToRemoveInviteSuccess()
    {
        var inviteGuid = Guid.NewGuid();
        _userRemovalService.Setup(s =>
                s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemoveUserViewModel.Empty with { OrganisationName = "Org" });
        _userRemovalService.Setup(s =>
                s.RemoveInviteAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InviteRemovalSubmitResult.Removed());

        var result = await _controller.RemoveInvite(TestOrganisationData.Id, inviteGuid,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true },
            CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(RemovalController.RemoveInviteSuccess));
    }

    [Fact]
    public async Task RemoveInvite_Post_WhenConfirmedButFails_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        _userRemovalService.Setup(s =>
                s.GetInviteViewModelAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RemoveUserViewModel.Empty with { OrganisationName = "Org" });
        _userRemovalService.Setup(s =>
                s.RemoveInviteAsync(TestOrganisationData.Id, inviteGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InviteRemovalSubmitResult.NotFound());

        var result = await _controller.RemoveInvite(TestOrganisationData.Id, inviteGuid,
            RemoveUserViewModel.Empty with { RemoveConfirmed = true },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveInviteSuccess_WhenStateAvailableForOrg_ReturnsView()
    {
        var inviteGuid = Guid.NewGuid();
        var state = new RemoveInviteSuccessState
            { OrganisationId = TestOrganisationData.Id, UserDisplayName = "John Doe" };
        _removeInviteStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.RemoveInviteSuccess(TestOrganisationData.Id, inviteGuid);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(RemovalController.RemoveSuccess));
        _removeInviteStateStore.Verify(s => s.ClearAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveInviteSuccess_WhenNoState_RedirectsToIndex()
    {
        var inviteGuid = Guid.NewGuid();
        _removeInviteStateStore.Setup(s => s.GetAsync()).ReturnsAsync((RemoveInviteSuccessState?)null);

        var result = await _controller.RemoveInviteSuccess(TestOrganisationData.Id, inviteGuid);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersListController.Index));
    }

    [Fact]
    public async Task RemoveInviteSuccess_WhenStateIsForDifferentOrg_RedirectsToIndex()
    {
        var inviteGuid = Guid.NewGuid();
        var state = new RemoveInviteSuccessState
            { OrganisationId = TestOrganisationData.OtherId, UserDisplayName = "John Doe" };
        _removeInviteStateStore.Setup(s => s.GetAsync()).ReturnsAsync(state);

        var result = await _controller.RemoveInviteSuccess(TestOrganisationData.Id, inviteGuid);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should()
            .Be(nameof(UsersListController.Index));
    }

    [Fact]
    public async Task RemoveApplication_Get_PassesCorrectParametersToService()
    {
        var cdpPersonId = Guid.NewGuid();
        const string clientId = "test-app";
        var viewModel = new RemoveApplicationViewModel(
            OrganisationId: TestOrganisationData.Id,
            UserDisplayName: "Jane Doe",
            UserEmail: "jane@example.com",
            ApplicationName: "Finance App",
            ApplicationSlug: clientId,
            AssignmentId: 456,
            OrgId: 1,
            UserPrincipalId: "principal-2",
            RoleName: "Editor",
            CdpPersonId: cdpPersonId);

        _userRemovalService.Setup(service =>
                service.GetRemoveApplicationViewModelAsync(TestOrganisationData.Id, cdpPersonId, clientId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        await _controller.RemoveApplication(TestOrganisationData.Id, cdpPersonId, clientId, CancellationToken.None);

        _userRemovalService.Verify(
            service => service.GetRemoveApplicationViewModelAsync(TestOrganisationData.Id, cdpPersonId, clientId,
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveApplication_Post_WhenNotConfirmed_RedirectsToDetails()
    {
        var cdpPersonId = Guid.NewGuid();
        var input = new RemoveApplicationViewModel(RevokeConfirmed: false);

        var result = await _controller.RemoveApplication(TestOrganisationData.Id, cdpPersonId, "test-app", input,
            CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UserDetailsController.Details));
        redirectResult.RouteValues.Should().Contain("id", TestOrganisationData.Id);
        redirectResult.RouteValues.Should().Contain("cdpPersonId", cdpPersonId);
        _userRemovalService.Verify(
            service => service.RemoveApplicationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveApplication_Post_WhenModelStateInvalidAndViewModelAvailable_ReturnsRemoveView()
    {
        var cdpPersonId = Guid.NewGuid();
        var viewModel = new RemoveApplicationViewModel(
            OrganisationId: TestOrganisationData.Id,
            UserDisplayName: "John Doe",
            UserEmail: "john@example.com",
            ApplicationName: "Test App",
            ApplicationSlug: "test-app",
            AssignmentId: 123,
            OrgId: 1,
            UserPrincipalId: "principal-1",
            RoleName: "Admin",
            CdpPersonId: cdpPersonId);

        _userRemovalService.Setup(service => service.GetRemoveApplicationViewModelAsync(TestOrganisationData.Id,
                cdpPersonId, "test-app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);
        _controller.ModelState.AddModelError(nameof(RemoveApplicationViewModel.RevokeConfirmed),
            "Select if you want to revoke access");

        var input = new RemoveApplicationViewModel(RevokeConfirmed: true);
        var result = await _controller.RemoveApplication(TestOrganisationData.Id, cdpPersonId, "test-app", input,
            CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(RemovalController.RemoveApplication));
        viewResult.Model.Should().Be(viewModel);
        _userRemovalService.Verify(
            service => service.RemoveApplicationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveApplication_Post_WhenModelStateInvalidAndViewModelMissing_ReturnsNotFound()
    {
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(service => service.GetRemoveApplicationViewModelAsync(TestOrganisationData.Id,
                cdpPersonId, "test-app", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveApplicationViewModel?)null);
        _controller.ModelState.AddModelError(nameof(RemoveApplicationViewModel.RevokeConfirmed),
            "Select if you want to revoke access");

        var input = new RemoveApplicationViewModel(RevokeConfirmed: true);
        var result = await _controller.RemoveApplication(TestOrganisationData.Id, cdpPersonId, "test-app", input,
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        _userRemovalService.Verify(
            service => service.RemoveApplicationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── RemoveApplicationSuccess ──────────────────────────────────────────────

    [Fact]
    public async Task RemoveApplicationSuccess_Get_WhenViewModelNull_RedirectsToIndex()
    {
        var userId = Guid.NewGuid();
        _userDetailsQueryService.Setup(s =>
                s.GetRemoveApplicationSuccessViewModelAsync(TestOrganisationData.Id, userId, "app-slug-1",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveApplicationSuccessViewModel?)null);

        var result =
            await _controller.RemoveApplicationSuccess(TestOrganisationData.Id, userId, "app-slug-1",
                CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersListController.Index));
        redirect.RouteValues!["id"].Should().Be(TestOrganisationData.Id);
    }

    [Fact]
    public async Task RemoveApplicationSuccess_Get_WhenViewModelValid_ReturnsSuccessView()
    {
        var userId = Guid.NewGuid();
        var viewModel = new RemoveApplicationSuccessViewModel
        {
            OrganisationId = TestOrganisationData.Id, UserDisplayName = "John Doe",
            Email = "john@example.com", ApplicationName = "Test App", CdpPersonId = userId
        };
        _userDetailsQueryService.Setup(s =>
                s.GetRemoveApplicationSuccessViewModelAsync(TestOrganisationData.Id, userId, "app-slug-2",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result =
            await _controller.RemoveApplicationSuccess(TestOrganisationData.Id, userId, "app-slug-2",
                CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be(nameof(RemovalController.RemoveApplicationSuccess));
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
            OrganisationId = TestOrganisationData.Id, UserDisplayName = "Jane Smith",
            Email = "jane@example.com", ApplicationName = "Finance App", CdpPersonId = userId
        };
        _userDetailsQueryService.Setup(s =>
                s.GetRemoveApplicationSuccessViewModelAsync(TestOrganisationData.Id, userId, appId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        await _controller.RemoveApplicationSuccess(TestOrganisationData.Id, userId, appId, CancellationToken.None);

        _userDetailsQueryService.Verify(
            s => s.GetRemoveApplicationSuccessViewModelAsync(TestOrganisationData.Id, userId, appId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveSuccess_WhenViewModelIsNull_RedirectsToIndex()
    {
        var organisationId = TestOrganisationData.Id;
        var cdpPersonId = Guid.NewGuid();
        _userRemovalService.Setup(s => s.GetRemoveSuccessViewModelAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoveSuccessViewModel?)null);

        var result = await _controller.RemoveSuccess(organisationId, cdpPersonId, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(UsersListController.Index));
        redirectResult.RouteValues!["id"].Should().Be(organisationId);
    }

    [Fact]
    public async Task RemoveSuccess_WithValidViewModelContainingAllData_ReturnsCorrectViewModel()
    {
        var organisationId = TestOrganisationData.Id;
        var cdpPersonId = Guid.NewGuid();
        var viewModel = new RemoveSuccessViewModel
        {
            OrganisationId = TestOrganisationData.Id,
            UserDisplayName = "Robert Johnson",
            Email = "robert.johnson@example.com",
            OrganisationName = "ACME Corporation",
            Role = OrganisationRole.Owner,
            MemberSince = "01 March 2023",
            CdpPersonId = cdpPersonId
        };
        _userRemovalService.Setup(s => s.GetRemoveSuccessViewModelAsync(
                organisationId,
                cdpPersonId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(viewModel);

        var result = await _controller.RemoveSuccess(organisationId, cdpPersonId, CancellationToken.None);

        var viewResult = (ViewResult)result;
        var returnedModel = (RemoveSuccessViewModel)viewResult.Model!;
        returnedModel.OrganisationId.Should().Be(TestOrganisationData.Id);
        returnedModel.UserDisplayName.Should().Be("Robert Johnson");
        returnedModel.Email.Should().Be("robert.johnson@example.com");
        returnedModel.OrganisationName.Should().Be("ACME Corporation");
        returnedModel.Role.Should().Be(OrganisationRole.Owner);
        returnedModel.MemberSince.Should().Be("01 March 2023");
        returnedModel.CdpPersonId.Should().Be(cdpPersonId);
    }
}