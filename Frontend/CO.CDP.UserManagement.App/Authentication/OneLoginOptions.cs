using System.ComponentModel.DataAnnotations;

namespace CO.CDP.UserManagement.App.Authentication;

public class OneLoginOptions
{
    [Required]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string PrivateKey { get; init; } = string.Empty;
}
