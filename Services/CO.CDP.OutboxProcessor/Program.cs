using CO.CDP.AwsServices;
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

// TODO:
// 1. connect to database
// 2. Resend existing messages when published is false
// 3. Remove OutboxProcessorListenerBackgroundService from other services
// 4. config for other dbs in compose / override
// 5. unit tests
// 6.

var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "OutboxDatabase");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());
//builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());

// EntityVerificationContext needs to be new context??
builder.Services.AddScoped<IOutboxMessageRepository, DatabaseOutboxMessageRepository<EntityVerificationContext>>();
builder.Services.AddScoped<IPublisher, OutboxMessagePublisher>();
builder.Services.AddScoped<IOutboxProcessor>(s =>
{
    return new OutboxProcessor(
        s.GetRequiredKeyedService<IPublisher>("SqsPublisher"),
        s.GetRequiredService<IOutboxMessageRepository>(),
        s.GetRequiredService<ILogger<OutboxProcessor>>()
    );
});

//builder.Services.AddScoped<IOutboxProcessorListener>(s => new OutboxProcessorListener(
//    s.GetRequiredService<NpgsqlDataSource>(),
//    s.GetRequiredService<IOutboxProcessor>(),
//    s.GetRequiredService<ILogger<OutboxProcessorListener>>(),
//    channel: builder.Configuration["channel"]!
//));

//builder.Services.AddSingleton<OutboxProcessorBackgroundService.OutboxProcessorConfiguration>(s =>
//         s.GetRequiredService<IOptions<AwsConfiguration>>().Value.SqsPublisher?.Outbox ??
//         new OutboxProcessorBackgroundService.OutboxProcessorConfiguration()
//     );

//builder.Services.AddHostedService<OutboxProcessorListenerBackgroundService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePages();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public abstract partial class Program;
