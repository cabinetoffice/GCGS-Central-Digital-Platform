using CO.CDP.Organisation.Authority;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var issuer = builder.Configuration["Issuer"]
    ?? throw new Exception("Missing configuration key: Issuer.");
var publicKey = builder.Configuration["PublicKey"]
    ?? throw new Exception("Missing configuration key: PublicKey.");
var privateKey = builder.Configuration["PrivateKey"]
    ?? throw new Exception("Missing configuration key: PrivateKey.");
var oneLoginAuthority = builder.Configuration["OneLogin:Authority"]
    ?? throw new Exception("Missing configuration key: OneLogin:Authority.");

var rsaPrivate = RSA.Create();
rsaPrivate.ImportFromPem(privateKey);
var rsaPrivateKey = new RsaSecurityKey(rsaPrivate);

var rsaPublic = RSA.Create();
rsaPublic.ImportFromPem(publicKey);
var resPublicParams = rsaPublic.ExportParameters(false);

var oneLoginConfiguration = GetOpenIdConnectConfigurationAsync(oneLoginAuthority).GetAwaiter().GetResult();

app.UseIdentity(issuer, rsaPrivateKey, resPublicParams, oneLoginConfiguration);

app.Run();

static async Task<OpenIdConnectConfiguration> GetOpenIdConnectConfigurationAsync(string authority)
{
    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
        new Uri(new Uri(authority), ".well-known/openid-configuration").ToString(),
        new OpenIdConnectConfigurationRetriever());

    return await configurationManager.GetConfigurationAsync();
}