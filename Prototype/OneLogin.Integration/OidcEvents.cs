using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace OneLogin.Integration;

public class OidcEvents(IConfiguration configuration) : OpenIdConnectEvents
{
    public override Task RedirectToIdentityProvider(RedirectContext context)
    {
        context.ProtocolMessage.Parameters.Add("vtr", "[\"Cl.Cm\"]");
        context.ProtocolMessage.Parameters.Add("ui_locales", "en");

        return Task.CompletedTask;
    }

    public override Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        context.TokenEndpointRequest!.ClientAssertionType = OidcConstants.ClientAssertionTypes.JwtBearer;
        context.TokenEndpointRequest.ClientAssertion = CreateClientToken();

        return Task.CompletedTask;
    }

    private string CreateClientToken()
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText("cdp-private.pem"));

        var credential = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };

        var clientId = configuration.GetValue<string>("OneLogin:ClientId")!;
        var authority = configuration.GetValue<string>("OneLogin:Authority")!;

        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            clientId,
            $"{authority}/token",
            new List<Claim>()
            {
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString()),
                new Claim(JwtClaimTypes.Subject, clientId),
                new Claim(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(), ClaimValueTypes.Integer64)
            },
            now,
            now.AddMinutes(1),
            credential
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}
