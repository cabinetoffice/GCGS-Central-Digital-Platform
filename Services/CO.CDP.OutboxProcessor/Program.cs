using CO.CDP.AwsServices;
using CO.CDP.Configuration.Helpers;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.OrganisationInformation.Persistence;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["channel"] == null)
{
    throw new ArgumentException("Missing channel configuration");
}

if (builder.Configuration["DbContext"] == null)
{
    throw new ArgumentException("Missing DbContext configuration");
}

builder.Services.AddSingleton(_ => new NpgsqlDataSourceBuilder(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OutboxDatabase"))
    .Build());
builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();
builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsSqsService()
    .AddLoggingConfiguration(builder.Configuration)
    .AddAmazonCloudWatchLogsService()
    .AddCloudWatchSerilog(builder.Configuration);

var context = builder.Configuration.GetValue<string>("DbContext");
if (context == "OrganisationInformationContext")
{
    builder.Services.AddOutboxSqsPublisher<OrganisationInformationContext>(builder.Configuration);
}
else
{
    builder.Services.AddOutboxSqsPublisher<EntityVerificationContext>(builder.Configuration);
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePages();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public abstract partial class Program;
