using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.Organisation.Authority.Model;

public class RevocationRequest
{
    [FromForm(Name = "token")]
    public required string Token { get; set; }

    [FromForm(Name = "token_type_hint")]
    public string? TokenTypeHint { get; set; }
}