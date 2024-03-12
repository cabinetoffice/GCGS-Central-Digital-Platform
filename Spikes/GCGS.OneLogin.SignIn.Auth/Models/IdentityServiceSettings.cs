namespace GCGS.OneLogin.SignIn.Auth.Models;

public class OneLoginSettings
{
    public string? AuthorityUrl { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public IEnumerable<string>? Scopes { get; set; }
    public string? CallbackPath { get; set; }
}
