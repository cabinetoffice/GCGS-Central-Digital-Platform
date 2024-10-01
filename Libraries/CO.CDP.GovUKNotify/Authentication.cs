using IdentityModel;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace CO.CDP.GovUKNotify;

public interface IAuthentication
{
    AuthenticationHeaderValue GetAuthenticationHeader();
}

public class Authentication(IConfiguration configuration) : IAuthentication
{
    private const string RegexGuid = @"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}";

    private string _serviceId = "";
    private string _secretKey = "";

    public AuthenticationHeaderValue GetAuthenticationHeader()
    {
        if (string.IsNullOrWhiteSpace(_serviceId) || string.IsNullOrWhiteSpace(_secretKey))
        {
            SetupApiConfiguration();
        }

        var credential = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)), SecurityAlgorithms.HmacSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };

        var token = new JwtSecurityToken(
            issuer: _serviceId,
            signingCredentials: credential,
            claims: [new Claim(JwtClaimTypes.IssuedAt, DateTime.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer64)]);

        var encodedJwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthenticationHeaderValue("Bearer", encodedJwtToken);
    }

    private void SetupApiConfiguration()
    {
        var apiKey = configuration.GetValue<string>("GOVUKNotify:ApiKey")
                   ?? throw new Exception("Missing configuration key: GOVUKNotify:ApiKey.");

        var matches = Regex.Matches(apiKey, RegexGuid);
        if (matches.Count != 2)
        {
            throw new Exception("Invalid configuration key: GOVUKNotify:ApiKey.");
        }

        _serviceId = matches[0].Value;
        _secretKey = matches[1].Value;
    }
}