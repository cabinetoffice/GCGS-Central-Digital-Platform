using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.RegisterOfCommercialTools.WebApi;
using CO.CDP.RegisterOfCommercialTools.WebApi.Constants;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureForwardedHeaders();

builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.DocumentCommercialToolsApi(builder.Configuration));
builder.Services.AddHealthChecks();

builder.Services.AddAuthorization();
builder.Services.AddAwsCognitoAuthentication(builder.Configuration, builder.Environment);

builder.Services.AddFeatureManagement(builder.Configuration.GetSection("Features"));

builder.Services.AddTransient<ICommercialToolsQueryBuilder, CommercialToolsQueryBuilder>();
builder.Services.AddHttpClient<ICommercialToolsRepository, CommercialToolsRepository>((serviceProvider, client) =>
{
    client.BaseAddress = new Uri(serviceProvider.GetRequiredService<IConfiguration>().GetSection("ODataApi:BaseUrl").Value ?? throw new InvalidOperationException("ODataApi:BaseUrl is not configured."));
});

builder.Services.AddTransient<ISearchService, SearchService>();

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddLoggingConfiguration(builder.Configuration)
    .AddAmazonCloudWatchLogsService()
    .AddCloudWatchSerilog(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var featureManager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();
    if (!await featureManager.IsEnabledAsync(FeatureFlags.CommercialTools))
    {
        app.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Not Found\",\"message\":\"The requested service is not available\"}");
        });
    }
}

app.UseForwardedHeaders();

if (builder.Environment.IsDevelopment() || builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePages();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();
app.Run();
public abstract partial class Program;