using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.DataSharing.WebApi.Api;
using CO.CDP.DataSharing.WebApi.AutoMapper;
using CO.CDP.DataSharing.WebApi.Extensions;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentDataSharingApi(builder.Configuration); });
builder.Services.AddHealthChecks()
    .AddNpgSql(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase"));
builder.Services.AddAutoMapper(typeof(DataSharingProfile));

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase")));

builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();
builder.Services.AddScoped<IFormRepository, DatabaseFormRepository>();
builder.Services.AddScoped<IUseCase<ShareRequest, ShareReceipt>, GenerateShareCodeUseCase>();

builder.Services.AddScoped<IUseCase<Guid, List<SharedConsent>?>, GetShareCodesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, string), SharedConsentDetails?>, GetShareCodeDetailsUseCase>();
builder.Services.AddDataSharingProblemDetails();
builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
//builder.Services.AddAuthorization();
builder.Services.AddOrganisationAuthorization();


builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsS3Service();

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseDataSharingEndpoints();

app.Run();

public abstract partial class Program;