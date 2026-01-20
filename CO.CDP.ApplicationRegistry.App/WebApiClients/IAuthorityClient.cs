using CO.CDP.ApplicationRegistry.Core.Tokens;
using CO.CDP.Functional;

namespace CO.CDP.ApplicationRegistry.App.WebApiClients;

public interface IAuthorityClient
{
    Task<Result<AuthorityClientError, AuthTokens>> GetAuthTokens(string? userUrn);

    Task<Result<AuthorityClientError, Unit>> RevokeRefreshToken(string? userUrn);
}

