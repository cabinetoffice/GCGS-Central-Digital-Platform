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

    [Required]
    public string? FirstName { get; init; }

    [Required]
    public string? LastName { get; init; }

    [Required]
    [EmailAddress]
    public string? Email { get; init; }

    public OrganisationRole? OrganisationRole { get; init; }

    public static InviteUserViewModel Empty => new();
}
