using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.Organisation.Authority.Model;

public class TokenRequest
{
    [FromForm(Name = "grant_type")]
    public required string GrantType { get; set; }

    [FromForm(Name = "client_secret")]
    public string? ClientSecret { get; set; }

    [FromForm(Name = "refresh_token")]
    public string? RefreshToken { get; set; }
}