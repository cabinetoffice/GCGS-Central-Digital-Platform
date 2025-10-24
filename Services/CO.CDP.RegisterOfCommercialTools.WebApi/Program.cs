using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.RegisterOfCommercialTools.WebApi;
using CO.CDP.RegisterOfCommercialTools.WebApi.AutoMapper;
using CO.CDP.RegisterOfCommercialTools.WebApi.Constants;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services.Caching;
using CO.CDP.RegisterOfCommercialTools.WebApi.Middleware;
using CO.CDP.RegisterOfCommercialTools.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureForwardedHeaders();

builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.DocumentCommercialToolsApi(builder.Configuration));
builder.Services.AddHealthChecks();

builder.Services.AddPassThroughAuthentication();

builder.Services.AddFeatureManagement(builder.Configuration.GetSection("Features"));

builder.Services.AddTransient<ICommercialToolsQueryBuilder, CommercialToolsQueryBuilder>();
builder.Services.AddHttpClient<ICommercialToolsService, CommercialToolsService>((serviceProvider, client) =>
{
    client.BaseAddress = new Uri(serviceProvider.GetRequiredService<IConfiguration>().GetSection("ODataApi:BaseUrl").Value ?? throw new InvalidOperationException("ODataApi:BaseUrl is not configured."));
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    var awsConfig = builder.Configuration.GetSection("Aws").Get<AwsConfiguration>();
    if (awsConfig?.ElastiCache != null)
    {
        options.Configuration = $"{awsConfig.ElastiCache.Hostname}:{awsConfig.ElastiCache.Port}";
        options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName") ?? "RoctCache:";
    }
});

builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<ISearchService>(provider =>
{
    var innerService = provider.GetRequiredService<SearchService>();
    var cacheService = provider.GetRequiredService<IRedisCacheService>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<CachedSearchService>>();

    return new CachedSearchService(innerService, cacheService, configuration, logger);
});

builder.Services.AddAutoMapper(typeof(ApiResponseProfile));


var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
builder.Services.AddHealthChecks()
    .AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>())
    .AddRedis(sp =>
    {
        var awsConfig = sp.GetRequiredService<IConfiguration>().GetSection("Aws").Get<AwsConfiguration>();
        return awsConfig?.ElastiCache != null
            ? $"{awsConfig.ElastiCache.Hostname}:{awsConfig.ElastiCache.Port}"
            : "localhost:6379";
    });
builder.Services.AddDbContext<RegisterOfCommercialToolsContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddScoped<ICpvCodeRepository, DatabaseCpvCodeRepository>();
builder.Services.AddScoped<INutsCodeRepository, DatabaseNutsCodeRepository>();

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
app.UseMiddleware<ExceptionMiddleware>();

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
