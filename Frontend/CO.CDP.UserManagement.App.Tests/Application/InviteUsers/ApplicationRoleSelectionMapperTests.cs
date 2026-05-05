using System;
using CO.CDP.UserManagement.App.Application.InviteUsers.Implementations;
using CO.CDP.UserManagement.App.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.UserManagement.App.Tests.Application.InviteUsers;

public class ApplicationRoleSelectionMapperTests
{
    private static readonly ApplicationRoleSelectionMapper Sut = new();

    private static ApplicationRolesStepViewModel BuildServerViewModel(
        params ApplicationAccessSelectionViewModel[] apps) =>
        new()
        {
            OrganisationId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Applications = apps.ToList()
        };

    private static ApplicationAccessSelectionViewModel SingleRoleApp(
        int appId = 1, bool multiRole = false, bool isDefault = false) =>
        new()
        {
            OrganisationApplicationId = appId,
            ApplicationName = $"App {appId}",
            AllowsMultipleRoleAssignments = multiRole,
            IsEnabledByDefault = isDefault,
            Roles = [new ApplicationRoleOptionViewModel { Id = 10, Name = "Viewer" }]
        };

    // ── MergePostedSelections ──────────────────────────────────────────────────

    [Fact]
    public void MergePostedSelections_PreservesServerMetadata()
    {
        var serverVm = BuildServerViewModel(SingleRoleApp(1));
        var post = new ApplicationRolesStepPostModel
        {
            Applications = [new ApplicationSelectionPostModel { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 10 }]
        };

        var result = Sut.MergePostedSelections(serverVm, post);

        result.OrganisationId.Should().Be(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"));
        result.FirstName.Should().Be("Jane");
        result.Applications.Should().HaveCount(1);
    }

    [Fact]
    public void MergePostedSelections_SingleRole_AppliesPostedSelection()
    {
        var serverVm = BuildServerViewModel(SingleRoleApp(1));
        var post = new ApplicationRolesStepPostModel
        {
            Applications = [new ApplicationSelectionPostModel { OrganisationApplicationId = 1, GiveAccess = true, SelectedRoleId = 10 }]
        };

        var result = Sut.MergePostedSelections(serverVm, post);

        var app = result.Applications.Single();
        app.GiveAccess.Should().BeTrue();
        app.SelectedRoleId.Should().Be(10);
    }

    [Fact]
    public void MergePostedSelections_MultiRole_AppliesPostedRoleIdsList()
    {
        var serverVm = BuildServerViewModel(SingleRoleApp(1, multiRole: true));
        var post = new ApplicationRolesStepPostModel
        {
            Applications = [new ApplicationSelectionPostModel
            {
                OrganisationApplicationId = 1,
                GiveAccess = true,
                SelectedRoleIds = [10, 20]
            }]
        };

        var result = Sut.MergePostedSelections(serverVm, post);

        var app = result.Applications.Single();
        app.SelectedRoleIds.Should().Equal(10, 20);
        app.SelectedRoleId.Should().Be(10);
    }

    [Fact]
    public void MergePostedSelections_IsEnabledByDefault_ForceGrantsAccess()
    {
        var serverVm = BuildServerViewModel(SingleRoleApp(1, isDefault: true));
        var post = new ApplicationRolesStepPostModel
        {
            Applications = [new ApplicationSelectionPostModel { OrganisationApplicationId = 1, GiveAccess = false }]
        };

        var result = Sut.MergePostedSelections(serverVm, post);

        result.Applications.Single().GiveAccess.Should().BeTrue();
    }

    [Fact]
    public void MergePostedSelections_AppNotPosted_KeepsServerState()
    {
        var serverVm = BuildServerViewModel(
            new ApplicationAccessSelectionViewModel
            {
                OrganisationApplicationId = 99,
                ApplicationName = "Hidden App",
                GiveAccess = true,
                SelectedRoleId = 5,
                IsEnabledByDefault = false
            });
        var post = new ApplicationRolesStepPostModel { Applications = [] };

        var result = Sut.MergePostedSelections(serverVm, post);

        var app = result.Applications.Single();
        app.GiveAccess.Should().BeTrue();
        app.SelectedRoleId.Should().Be(5);
    }

    // ── ApplyExistingSelections ────────────────────────────────────────────────

    [Fact]
    public void ApplyExistingSelections_WithNullSaved_ReturnsServerViewModelUnchanged()
    {
        var serverVm = BuildServerViewModel(SingleRoleApp(1));

        var result = Sut.ApplyExistingSelections(serverVm, null);

        result.Should().BeSameAs(serverVm);
    }

    [Fact]
    public void ApplyExistingSelections_WithEmptySaved_ReturnsServerViewModelUnchanged()
    {
        var serverVm = BuildServerViewModel(SingleRoleApp(1));

        var result = Sut.ApplyExistingSelections(serverVm, []);

        result.Should().BeSameAs(serverVm);
    }

    [Fact]
    public void ApplyExistingSelections_SingleRole_RestoresSavedApplicationRoleId()
    {
        var serverVm = BuildServerViewModel(SingleRoleApp(1));
        var saved = new List<InviteApplicationAssignment>
        {
            new() { OrganisationApplicationId = 1, ApplicationRoleId = 10 }
        };

        var result = Sut.ApplyExistingSelections(serverVm, saved);

        var app = result.Applications.Single();
        app.GiveAccess.Should().BeTrue();
        app.SelectedRoleId.Should().Be(10);
        app.SelectedRoleIds.Should().Equal(10);
    }

    [Fact]
    public void ApplyExistingSelections_MultiRole_RestoresFullRoleIdsList()
    {
        // This is the multi-role bug-fix case: ApplicationRoleIds must be restored, not just ApplicationRoleId
        var serverVm = BuildServerViewModel(SingleRoleApp(1, multiRole: true));
        var saved = new List<InviteApplicationAssignment>
        {
            new() { OrganisationApplicationId = 1, ApplicationRoleId = 10, ApplicationRoleIds = [10, 20, 30] }
        };

        var result = Sut.ApplyExistingSelections(serverVm, saved);

        var app = result.Applications.Single();
        app.GiveAccess.Should().BeTrue();
        app.SelectedRoleIds.Should().Equal(10, 20, 30);
        app.SelectedRoleId.Should().Be(10);
    }

    [Fact]
    public void ApplyExistingSelections_MultiRole_FallsBackToApplicationRoleIdWhenNoList()
    {
        var serverVm = BuildServerViewModel(SingleRoleApp(1, multiRole: true));
        var saved = new List<InviteApplicationAssignment>
        {
            new() { OrganisationApplicationId = 1, ApplicationRoleId = 42, ApplicationRoleIds = null }
        };

        var result = Sut.ApplyExistingSelections(serverVm, saved);

        var app = result.Applications.Single();
        app.SelectedRoleIds.Should().Equal(42);
        app.SelectedRoleId.Should().Be(42);
    }

    [Fact]
    public void ApplyExistingSelections_AppNotInSaved_RetainsOriginalAppUnchanged()
    {
        var serverVm = BuildServerViewModel(SingleRoleApp(1), SingleRoleApp(2));
        var saved = new List<InviteApplicationAssignment>
        {
            new() { OrganisationApplicationId = 1, ApplicationRoleId = 10 }
        };

        var result = Sut.ApplyExistingSelections(serverVm, saved);

        result.Applications[0].GiveAccess.Should().BeTrue();   // app 1 restored
        result.Applications[1].GiveAccess.Should().BeFalse();  // app 2 not in saved — untouched
    }

    // ── ValidateSelections ─────────────────────────────────────────────────────

    [Fact]
    public void ValidateSelections_WhenNoAppsSelected_AddsModelError()
    {
        var vm = BuildServerViewModel(
            new ApplicationAccessSelectionViewModel { OrganisationApplicationId = 1, GiveAccess = false });
        var modelState = new ModelStateDictionary();

        var valid = Sut.ValidateSelections(vm, modelState);

        valid.Should().BeFalse();
        modelState.Should().ContainKey("applicationSelections");
    }

    [Fact]
    public void ValidateSelections_WhenSelectedAppHasNoRole_AddsModelError()
    {
        var vm = BuildServerViewModel(
            new ApplicationAccessSelectionViewModel
            {
                OrganisationApplicationId = 1,
                ApplicationName = "AppX",
                GiveAccess = true,
                SelectedRoleId = null,
                Roles = [new ApplicationRoleOptionViewModel { Id = 10, Name = "Viewer" }]
            });
        var modelState = new ModelStateDictionary();

        var valid = Sut.ValidateSelections(vm, modelState);

        valid.Should().BeFalse();
        modelState.Should().ContainKey("Applications[0].SelectedRoleId");
    }

    [Fact]
    public void ValidateSelections_WhenValidSingleRoleSelection_ReturnsTrue()
    {
        var vm = BuildServerViewModel(
            new ApplicationAccessSelectionViewModel
            {
                OrganisationApplicationId = 1,
                ApplicationName = "AppX",
                GiveAccess = true,
                SelectedRoleId = 10,
                Roles = [new ApplicationRoleOptionViewModel { Id = 10, Name = "Viewer" }]
            });
        var modelState = new ModelStateDictionary();

        var valid = Sut.ValidateSelections(vm, modelState);

        valid.Should().BeTrue();
        modelState.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateSelections_WhenValidMultiRoleSelection_ReturnsTrue()
    {
        var vm = BuildServerViewModel(
            new ApplicationAccessSelectionViewModel
            {
                OrganisationApplicationId = 1,
                ApplicationName = "AppX",
                AllowsMultipleRoleAssignments = true,
                GiveAccess = true,
                SelectedRoleIds = [10, 20],
                Roles = [new ApplicationRoleOptionViewModel { Id = 10, Name = "Viewer" }]
            });
        var modelState = new ModelStateDictionary();

        var valid = Sut.ValidateSelections(vm, modelState);

        valid.Should().BeTrue();
    }

    // ── MapToAssignments ───────────────────────────────────────────────────────

    [Fact]
    public void MapToAssignments_SingleRole_MapsToAssignmentWithRoleId()
    {
        var apps = new List<ApplicationAccessSelectionViewModel>
        {
            new()
            {
                OrganisationApplicationId = 1,
                GiveAccess = true,
                AllowsMultipleRoleAssignments = false,
                SelectedRoleId = 10,
                Roles = [new ApplicationRoleOptionViewModel { Id = 10, Name = "Viewer" }]
            }
        };

        var result = Sut.MapToAssignments(apps);

        result.Should().HaveCount(1);
        result[0].OrganisationApplicationId.Should().Be(1);
        result[0].ApplicationRoleId.Should().Be(10);
        result[0].ApplicationRoleIds.Should().BeNull();
    }

    [Fact]
    public void MapToAssignments_MultiRole_MapsToAssignmentWithRoleIdsList()
    {
        var apps = new List<ApplicationAccessSelectionViewModel>
        {
            new()
            {
                OrganisationApplicationId = 1,
                GiveAccess = true,
                AllowsMultipleRoleAssignments = true,
                SelectedRoleIds = [10, 20],
                Roles = [new ApplicationRoleOptionViewModel { Id = 10, Name = "Viewer" }]
            }
        };

        var result = Sut.MapToAssignments(apps);

        result.Should().HaveCount(1);
        result[0].ApplicationRoleId.Should().Be(10);
        result[0].ApplicationRoleIds.Should().Equal(10, 20);
    }

    [Fact]
    public void MapToAssignments_ExcludesAppsWithNoRoleSelected()
    {
        var apps = new List<ApplicationAccessSelectionViewModel>
        {
            new()
            {
                OrganisationApplicationId = 1,
                GiveAccess = true,
                AllowsMultipleRoleAssignments = false,
                SelectedRoleId = null,
                Roles = [new ApplicationRoleOptionViewModel { Id = 10, Name = "Viewer" }]
            }
        };

        var result = Sut.MapToAssignments(apps);

        result.Should().BeEmpty();
    }

    [Fact]
    public void MapToAssignments_ExcludesMultiRoleAppsWithEmptyRoleIdsList()
    {
        var apps = new List<ApplicationAccessSelectionViewModel>
        {
            new()
            {
                OrganisationApplicationId = 1,
                GiveAccess = true,
                AllowsMultipleRoleAssignments = true,
                SelectedRoleIds = [],
                Roles = []
            }
        };

        var result = Sut.MapToAssignments(apps);

        result.Should().BeEmpty();
    }
}
