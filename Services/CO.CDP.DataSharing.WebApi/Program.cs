using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.DataSharing.WebApi;
using CO.CDP.DataSharing.WebApi.Api;
using CO.CDP.DataSharing.WebApi.AutoMapper;
using CO.CDP.DataSharing.WebApi.DataService;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.WebApi.Foundation;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Globalization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentDataSharingApi(builder.Configuration); });

builder.Services.AddLocalization();

// Note that we have to register IHtmlLocalizer manually, since this is an API and we are not calling AddViewLocalization
builder.Services.AddSingleton<IHtmlLocalizerFactory, HtmlLocalizerFactory>();
builder.Services.AddTransient(typeof(IHtmlLocalizer<>), typeof(HtmlLocalizer<>));

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("cy") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

builder.Services.AddTransient(typeof(DocumentUriValueResolver));
builder.Services.AddTransient(typeof(FormQuestionOptionsResolver));
builder.Services.AddTransient(typeof(LocalizedPropertyResolver<,>));
builder.Services.AddAutoMapper(typeof(DataSharingProfile));

var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());
builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());
builder.Services.AddDbContext<OrganisationInformationContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();
builder.Services.AddScoped<IFormRepository, DatabaseFormRepository>();
builder.Services.AddScoped<IShareCodeRepository, DatabaseShareCodeRepository>();
builder.Services.AddScoped<IConnectedEntityRepository, DatabaseConnectedEntityRepository>();
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IUseCase<ShareRequest, ShareReceipt>, GenerateShareCodeUseCase>();
builder.Services.AddScoped<IUseCase<Guid, List<SharedConsent>?>, GetShareCodesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, string), SharedConsentDetails?>, GetShareCodeDetailsUseCase>();
builder.Services.AddScoped<IUseCase<ShareVerificationRequest, ShareVerificationReceipt>, GetShareCodeVerifyUseCase>();
builder.Services.AddScoped<IUseCase<string, CO.CDP.DataSharing.WebApi.Model.SupplierInformation?>, GetSharedDataUseCase>();
builder.Services.AddScoped<IUseCase<string, SharedDataFile?>, GetSharedDataFileUseCase>();
builder.Services.AddScoped<IUseCase<(string, string), string?>, GetSharedDataDocumentDownloadUrlUseCase>();
builder.Services.AddProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddOrganisationAuthorization();

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsS3Service();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.DataSharing.WebApi"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration);
}

var app = builder.Build();
app.UseForwardedHeaders();
app.UseErrorHandler(ErrorCodes.Exception4xxMap);

// Configure the HTTP request pipeline.
if (builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestLocalization();
app.UseDataSharingEndpoints();

app.Run();

public abstract partial class Program;