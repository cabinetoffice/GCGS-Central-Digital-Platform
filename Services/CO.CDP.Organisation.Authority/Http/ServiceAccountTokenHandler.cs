using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace CO.CDP.Organisation.Authority.Http;

public sealed class ServiceAccountTokenHandler(IConfigurationService configService) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateServiceToken());
        return base.SendAsync(request, cancellationToken);
    }

    private string CreateServiceToken()
    {
        var config = configService.GetAuthorityConfiguration();
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("client_id", "organisation-authority"),
                new Claim("channel", "service-account")
            }),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = config.Issuer,
            SigningCredentials = new SigningCredentials(config.RsaPrivateKey, SecurityAlgorithms.RsaSha256)
        };

        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        token.Header["kid"] = config.Kid;
        return tokenHandler.WriteToken(token);
    }
}
