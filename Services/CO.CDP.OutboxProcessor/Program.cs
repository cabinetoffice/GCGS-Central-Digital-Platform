using CO.CDP.AwsServices;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.Configuration.Helpers;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.MQ;
using CO.CDP.MQ.Hosting;
using CO.CDP.MQ.Outbox;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();
builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsSqsService()
    .AddLoggingConfiguration(builder.Configuration)
    .AddAmazonCloudWatchLogsService()
    .AddCloudWatchSerilog(builder.Configuration);

if (builder.Configuration["channel"] == null)
{
    throw new ArgumentException("Missing channel configuration");
}

if (builder.Configuration["DbContext"] == null)
{
    throw new ArgumentException("Missing DbContext configuration");
}

builder.Services.AddSingleton(_ => new NpgsqlDataSourceBuilder(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OutboxDatabase")).Build());
builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());

var context = builder.Configuration.GetValue<string>("DbContext");
if (context == "OrganisationInformationContext")
{
    builder.Services.AddDbContext<OrganisationInformationContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
    builder.Services.AddScoped<IOutboxMessageRepository, DatabaseOutboxMessageRepository<OrganisationInformationContext>>();
}
else
{
    builder.Services.AddDbContext<EntityVerificationContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
    builder.Services.AddScoped<IOutboxMessageRepository, DatabaseOutboxMessageRepository<EntityVerificationContext>>();
}

builder.Services.AddSingleton<OutboxProcessorBackgroundService.OutboxProcessorConfiguration>(s =>
         s.GetRequiredService<IOptions<AwsConfiguration>>().Value.SqsPublisher?.Outbox ??
         new OutboxProcessorBackgroundService.OutboxProcessorConfiguration()
     );
builder.Services.AddKeyedScoped<IOutboxPublisher, SqsOutboxPublisher>("SqsOutboxPublisher");
builder.Services.AddScoped<IOutboxProcessor>(s =>
{
    return new OutboxProcessor(
        s.GetRequiredKeyedService<IOutboxPublisher>("SqsOutboxPublisher"),
        s.GetRequiredService<IOutboxMessageRepository>(),
        s.GetRequiredService<ILogger<OutboxProcessor>>()
    );
});


builder.Services.AddScoped<IOutboxProcessorListener>(s => new OutboxProcessorListener(
    s.GetRequiredService<NpgsqlDataSource>(),
    s.GetRequiredService<IOutboxProcessor>(),
    s.GetRequiredService<ILogger<OutboxProcessorListener>>(),
    channel: builder.Configuration["channel"]!
));
builder.Services.AddHostedService<OutboxProcessorListenerBackgroundService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePages();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public abstract partial class Program;
