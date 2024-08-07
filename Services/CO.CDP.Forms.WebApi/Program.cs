using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Forms.WebApi.Api;
using CO.CDP.Forms.WebApi.AutoMapper;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentFormsApi(builder.Configuration); });

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? "");
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? ""));
builder.Services.AddScoped<IFormRepository, DatabaseFormRepository>();
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();

builder.Services.AddScoped<IUseCase<(Guid, Guid), bool>, DeleteAnswerSetUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, Guid), CO.CDP.Forms.WebApi.Model.SectionQuestionsResponse?>, GetFormSectionQuestionsUseCase>();
builder.Services.AddScoped<IUseCase<(Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, List<CO.CDP.Forms.WebApi.Model.FormAnswer> answers), bool>, UpdateFormSectionAnswersUseCase>();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
//builder.Services.AddAuthorization();
builder.Services.AddOrganisationAuthorization();

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsS3Service();

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
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

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseFormsEndpoints();
app.Run();
public abstract partial class Program;