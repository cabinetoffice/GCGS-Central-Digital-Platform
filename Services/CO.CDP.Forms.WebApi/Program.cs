using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.Forms.WebApi.Api;
using CO.CDP.Forms.WebApi.AutoMapper;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentFormsApi(builder.Configuration); });

builder.Services.AddHealthChecks()
    .AddNpgSql(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase"));
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase")));
builder.Services.AddScoped<IFormRepository, DatabaseFormRepository>();
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();

builder.Services.AddScoped<IUseCase<(Guid, Guid), FormSectionResponse?>, GetFormSectionsUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), bool>, DeleteAnswerSetUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, Guid), SectionQuestionsResponse?>, GetFormSectionQuestionsUseCase>();
builder.Services.AddScoped<IUseCase<(Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, UpdateFormSectionAnswers updateFormSectionAnswers), bool>, UpdateFormSectionAnswersUseCase>();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddOrganisationAuthorization();

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsS3Service();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Forms.WebApi"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration);
}

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
app.UseFormsEndpoints();
app.Run();
public abstract partial class Program;