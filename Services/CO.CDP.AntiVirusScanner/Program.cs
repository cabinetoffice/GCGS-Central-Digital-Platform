
using CO.CDP.AntiVirusScanner;
using CO.CDP.AwsServices;
using CO.CDP.MQ.Hosting;
using CO.CDP.MQ;
using System.Reflection;
using CO.CDP.Configuration.Assembly;
using CO.CDP.WebApi.Foundation;

var builder = WebApplication.CreateBuilder(args);

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.AntiVirusScanner"))
{
    builder.Services.AddScoped<IScanner, Scanner>();
    builder.Services.AddHealthChecks();
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddAwsSqsService()
        .AddAwsS3Service()
        .AddSqsDispatcher(
            EventDeserializer.Deserializer,
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
    .AddCloudWatchSerilog(builder.Configuration);
}

var app = builder.Build();

app.UseErrorHandler(ErrorCodes.Exception4xxMap);
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePages();
app.MapHealthChecks("/health").AllowAnonymous();

//var scan = new Scanner();
//scan.Scan();

// call ClamAV with existing prod bucket file to scan

// if clean, move to prod bucket

// see if data share works when zero file exists, for case when file is virus

// check AV updates

// add logging. add to Grafana

// 


app.Run();

public abstract partial class Program;

public static class ErrorCodes
{
    public static readonly Dictionary<Type, (int, string)> Exception4xxMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") }
    };
}