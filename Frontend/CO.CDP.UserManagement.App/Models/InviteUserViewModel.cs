using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record InviteUserViewModel
{
    [BindNever]
    [ValidateNever]
    public string OrganisationName { get; init; } = string.Empty;

    [BindNever]
    [ValidateNever]
    public string OrganisationSlug { get; init; } = string.Empty;

    [Required(ErrorMessage = "Enter a first name")]
    public string? FirstName { get; init; }

    [Required(ErrorMessage = "Enter a last name")]
    public string? LastName { get; init; }

    [Required(ErrorMessage = "Enter an email address")]
    [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? Email { get; init; }

    public OrganisationRole? OrganisationRole { get; init; }

    public static InviteUserViewModel Empty => new();
}
