
using CO.CDP.AntiVirusScanner;
using CO.CDP.AwsServices;
using CO.CDP.GovUKNotify;
using CO.CDP.MQ;
using CO.CDP.MQ.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddScoped<IScanner, Scanner>();
builder.Services.AddHealthChecks();
builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsSqsService()
    .AddAwsS3Service()
    .AddSqsDispatcher(
        EventDeserializer.Deserializer,
        enableBackgroundServices: true,
        services =>
        {
            services.AddScoped<ISubscriber<ScanFile>, ScanFileSubscriber>();
        },
        (services, dispatcher) =>
        {
            dispatcher.Subscribe<ScanFile>(services);
        }
    );

builder.Services.AddHostedService<DispatcherBackgroundService>();

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddLoggingConfiguration(builder.Configuration)
    .AddAmazonCloudWatchLogsService()
    .AddCloudWatchSerilog(builder.Configuration)
    .AddGovUKNotifyApiClient(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePages();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public abstract partial class Program;
