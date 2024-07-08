using CO.CDP.Organisation.Authority.Model;

namespace CO.CDP.Organisation.Authority;

public interface ITokenService
{
    Task<TokenResponse> CreateToken(string urn);

    Task<(bool valid, string? urn)> ValidateOneLoginToken(string? token);
}