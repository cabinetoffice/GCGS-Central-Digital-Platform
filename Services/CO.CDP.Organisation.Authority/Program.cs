using CO.CDP.Organisation.Authority;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using CO.CDP.Configuration.ForwardedHeaders;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureForwardedHeaders();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => { o.DocumentApi(); });
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IOpenIdConfiguration, OneLoginConfiguration>();

var app = builder.Build();
app.UseForwardedHeaders();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

var issuer = builder.Configuration["Issuer"]
    ?? throw new Exception("Missing configuration key: Issuer.");
var publicKey = builder.Configuration["PublicKey"]
    ?? throw new Exception("Missing configuration key: PublicKey.");
var privateKey = builder.Configuration["PrivateKey"]
    ?? throw new Exception("Missing configuration key: PrivateKey.");

var rsaPrivate = RSA.Create();
rsaPrivate.ImportFromPem(privateKey);
var rsaPrivateKey = new RsaSecurityKey(rsaPrivate);

var rsaPublic = RSA.Create();
rsaPublic.ImportFromPem(publicKey);
var resPublicParams = rsaPublic.ExportParameters(false);

app.UseIdentity(issuer, rsaPrivateKey, resPublicParams);

app.Run();
public abstract partial class Program;