using CO.CDP.UserManagement.Api.Validation;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using FluentAssertions;

namespace CO.CDP.UserManagement.Api.Tests.Validation;

public class RequestValidatorsTests
{
    private const int NameMaxLength = 255;
    private const int DescriptionMaxLength = 1000;
    private const int EmailMaxLength = 255;
    private const int UserPrincipalIdMaxLength = 255;

    [Fact]
    public void CreateApplicationRequestValidator_WhenValid_Passes()
    {
        var validator = new CreateApplicationRequestValidator();
        var request = new CreateApplicationRequest
        {
            Name = "App",
            ClientId = "client",
            Description = "desc",
            IsActive = true
        };

        validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateApplicationRequestValidator_WhenNameMissing_Fails()
    {
        var validator = new CreateApplicationRequestValidator();
        var request = new CreateApplicationRequest
        {
            Name = "",
            ClientId = "client",
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(CreateApplicationRequest.Name));
    }

    [Fact]
    public void CreateApplicationRequestValidator_WhenClientIdMissing_Fails()
    {
        var validator = new CreateApplicationRequestValidator();
        var request = new CreateApplicationRequest
        {
            Name = "App",
            ClientId = "",
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(CreateApplicationRequest.ClientId));
    }

    [Fact]
    public void CreateApplicationRequestValidator_WhenDescriptionTooLong_Fails()
    {
        var validator = new CreateApplicationRequestValidator();
        var request = new CreateApplicationRequest
        {
            Name = "App",
            ClientId = "client",
            Description = LongString(DescriptionMaxLength + 1),
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(CreateApplicationRequest.Description));
    }

    [Fact]
    public void UpdateApplicationRequestValidator_WhenNameMissing_Fails()
    {
        var validator = new UpdateApplicationRequestValidator();
        var request = new UpdateApplicationRequest { Name = "", IsActive = true };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(UpdateApplicationRequest.Name));
    }

    [Fact]
    public void UpdateApplicationRequestValidator_WhenDescriptionTooLong_Fails()
    {
        var validator = new UpdateApplicationRequestValidator();
        var request = new UpdateApplicationRequest
        {
            Name = "App",
            Description = LongString(DescriptionMaxLength + 1),
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(UpdateApplicationRequest.Description));
    }

    [Fact]
    public void CreateOrganisationRequestValidator_WhenCdpGuidEmpty_Fails()
    {
        var validator = new CreateOrganisationRequestValidator();
        var request = new CreateOrganisationRequest
        {
            CdpOrganisationGuid = Guid.Empty,
            Name = "Org",
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(CreateOrganisationRequest.CdpOrganisationGuid));
    }

    [Fact]
    public void UpdateOrganisationRequestValidator_WhenNameTooLong_Fails()
    {
        var validator = new UpdateOrganisationRequestValidator();
        var request = new UpdateOrganisationRequest
        {
            Name = LongString(NameMaxLength + 1),
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(UpdateOrganisationRequest.Name));
    }

    [Fact]
    public void CreatePermissionRequestValidator_WhenNameMissing_Fails()
    {
        var validator = new CreatePermissionRequestValidator();
        var request = new CreatePermissionRequest
        {
            Name = "",
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(CreatePermissionRequest.Name));
    }

    [Fact]
    public void UpdatePermissionRequestValidator_WhenDescriptionTooLong_Fails()
    {
        var validator = new UpdatePermissionRequestValidator();
        var request = new UpdatePermissionRequest
        {
            Name = "Perm",
            Description = LongString(DescriptionMaxLength + 1),
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(UpdatePermissionRequest.Description));
    }

    [Fact]
    public void CreateRoleRequestValidator_WhenNameMissing_Fails()
    {
        var validator = new CreateRoleRequestValidator();
        var request = new CreateRoleRequest
        {
            Name = "",
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(CreateRoleRequest.Name));
    }

    [Fact]
    public void UpdateRoleRequestValidator_WhenDescriptionTooLong_Fails()
    {
        var validator = new UpdateRoleRequestValidator();
        var request = new UpdateRoleRequest
        {
            Name = "Role",
            Description = LongString(DescriptionMaxLength + 1),
            IsActive = true
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(UpdateRoleRequest.Description));
    }

    [Fact]
    public void AssignPermissionsRequestValidator_WhenEmpty_Fails()
    {
        var validator = new AssignPermissionsRequestValidator();
        var request = new AssignPermissionsRequest { PermissionIds = new List<int>() };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(AssignPermissionsRequest.PermissionIds));
    }

    [Fact]
    public void AssignPermissionsRequestValidator_WhenInvalidId_Fails()
    {
        var validator = new AssignPermissionsRequestValidator();
        var request = new AssignPermissionsRequest { PermissionIds = new List<int> { 0 } };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, "PermissionIds[0]");
    }

    [Fact]
    public void AssignUserToApplicationRequestValidator_WhenInvalid_Fails()
    {
        var validator = new AssignUserToApplicationRequestValidator();
        var request = new AssignUserToApplicationRequest
        {
            ApplicationId = 0,
            RoleIds = new List<int>()
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(AssignUserToApplicationRequest.ApplicationId));
        AssertHasError(result, nameof(AssignUserToApplicationRequest.RoleIds));
    }

    [Fact]
    public void UpdateAssignmentRolesRequestValidator_WhenInvalidRoleId_Fails()
    {
        var validator = new UpdateAssignmentRolesRequestValidator();
        var request = new UpdateAssignmentRolesRequest { RoleIds = new List<int> { 0 } };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, "RoleIds[0]");
    }

    [Fact]
    public void EnableApplicationRequestValidator_WhenInvalid_Fails()
    {
        var validator = new EnableApplicationRequestValidator();
        var request = new EnableApplicationRequest { ApplicationId = 0 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(EnableApplicationRequest.ApplicationId));
    }

    [Fact]
    public void InviteUserRequestValidator_WhenInvalid_Fails()
    {
        var validator = new InviteUserRequestValidator();
        var request = new InviteUserRequest
        {
            FirstName = "",
            LastName = "",
            Email = "not-an-email",
            OrganisationRole = (OrganisationRole)999
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(InviteUserRequest.FirstName));
        AssertHasError(result, nameof(InviteUserRequest.LastName));
        AssertHasError(result, nameof(InviteUserRequest.Email));
        AssertHasError(result, nameof(InviteUserRequest.OrganisationRole));
    }

    [Fact]
    public void ChangeOrganisationRoleRequestValidator_WhenInvalidRole_Fails()
    {
        var validator = new ChangeOrganisationRoleRequestValidator();
        var request = new ChangeOrganisationRoleRequest { OrganisationRole = (OrganisationRole)999 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(ChangeOrganisationRoleRequest.OrganisationRole));
    }

    [Fact]
    public void AcceptOrganisationInviteRequestValidator_WhenInvalid_Fails()
    {
        var validator = new AcceptOrganisationInviteRequestValidator();
        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = "",
            CdpPersonId = Guid.Empty
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        AssertHasError(result, nameof(AcceptOrganisationInviteRequest.UserPrincipalId));
        AssertHasError(result, nameof(AcceptOrganisationInviteRequest.CdpPersonId));
    }

    private static void AssertHasError(FluentValidation.Results.ValidationResult result, string propertyName)
    {
        result.Errors.Should().Contain(error => error.PropertyName == propertyName);
    }

    private static string LongString(int length)
    {
        return new string('a', length);
    }
}
