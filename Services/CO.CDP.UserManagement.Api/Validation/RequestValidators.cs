using CO.CDP.UserManagement.Shared.Requests;
using FluentValidation;

namespace CO.CDP.UserManagement.Api.Validation;

internal static class ValidationConstants
{
    public const int NameMaxLength = 255;
    public const int DescriptionMaxLength = 1000;
    public const int EmailMaxLength = 255;
    public const int UserPrincipalIdMaxLength = 255;
}

public sealed class CreateApplicationRequestValidator : AbstractValidator<CreateApplicationRequest>
{
    public CreateApplicationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.DescriptionMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public sealed class UpdateApplicationRequestValidator : AbstractValidator<UpdateApplicationRequest>
{
    public UpdateApplicationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.DescriptionMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public sealed class CreateOrganisationRequestValidator : AbstractValidator<CreateOrganisationRequest>
{
    public CreateOrganisationRequestValidator()
    {
        RuleFor(x => x.CdpOrganisationGuid).NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);
    }
}

public sealed class UpdateOrganisationRequestValidator : AbstractValidator<UpdateOrganisationRequest>
{
    public UpdateOrganisationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);
    }
}

public sealed class CreatePermissionRequestValidator : AbstractValidator<CreatePermissionRequest>
{
    public CreatePermissionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.DescriptionMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public sealed class UpdatePermissionRequestValidator : AbstractValidator<UpdatePermissionRequest>
{
    public UpdatePermissionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.DescriptionMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.DescriptionMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.DescriptionMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public sealed class AssignPermissionsRequestValidator : AbstractValidator<AssignPermissionsRequest>
{
    public AssignPermissionsRequestValidator()
    {
        RuleFor(x => x.PermissionIds)
            .NotEmpty();

        RuleForEach(x => x.PermissionIds)
            .GreaterThan(0);
    }
}

public sealed class AssignUserToApplicationRequestValidator : AbstractValidator<AssignUserToApplicationRequest>
{
    public AssignUserToApplicationRequestValidator()
    {
        RuleFor(x => x.ApplicationId)
            .GreaterThan(0);

        RuleFor(x => x.RoleIds)
            .NotEmpty();

        RuleForEach(x => x.RoleIds)
            .GreaterThan(0);
    }
}

public sealed class UpdateAssignmentRolesRequestValidator : AbstractValidator<UpdateAssignmentRolesRequest>
{
    public UpdateAssignmentRolesRequestValidator()
    {
        RuleFor(x => x.RoleIds)
            .NotEmpty();

        RuleForEach(x => x.RoleIds)
            .GreaterThan(0);
    }
}

public sealed class EnableApplicationRequestValidator : AbstractValidator<EnableApplicationRequest>
{
    public EnableApplicationRequestValidator()
    {
        RuleFor(x => x.ApplicationId)
            .GreaterThan(0);
    }
}

public sealed class InviteUserRequestValidator : AbstractValidator<InviteUserRequest>
{
    public InviteUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(ValidationConstants.EmailMaxLength);

        RuleFor(x => x.OrganisationRole)
            .IsInEnum();

        RuleForEach(x => x.ApplicationAssignments)
            .ChildRules(assignment =>
            {
                assignment.RuleFor(a => a.OrganisationApplicationId)
                    .GreaterThan(0);

                assignment.RuleFor(a => a.ApplicationRoleIds)
                    .NotEmpty();

                assignment.RuleForEach(a => a.ApplicationRoleIds)
                    .GreaterThan(0);
            });
    }
}

public sealed class ChangeOrganisationRoleRequestValidator : AbstractValidator<ChangeOrganisationRoleRequest>
{
    public ChangeOrganisationRoleRequestValidator()
    {
        RuleFor(x => x.OrganisationRole)
            .IsInEnum();
    }
}

public sealed class AcceptOrganisationInviteRequestValidator : AbstractValidator<AcceptOrganisationInviteRequest>
{
    public AcceptOrganisationInviteRequestValidator()
    {
        RuleFor(x => x.UserPrincipalId)
            .NotEmpty()
            .MaximumLength(ValidationConstants.UserPrincipalIdMaxLength);

        RuleFor(x => x.CdpPersonId)
            .NotEmpty();
    }
}
