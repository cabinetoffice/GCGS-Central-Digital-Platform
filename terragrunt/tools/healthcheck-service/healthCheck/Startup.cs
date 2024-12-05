using Amazon.SQS;
using healthCheck.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace healthCheck
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var awsOptions = Configuration.GetAWSOptions();
            awsOptions.Region = Amazon.RegionEndpoint.EUWest2;
            services.AddDefaultAWSOptions(awsOptions);

            services.AddAWSService<IAmazonSQS>();

            services.AddHealthChecks()
                .AddCheck<ElastiCacheHealthCheck>("ElastiCache", tags: new[] { "monitoring" })
                .AddCheck<SqsHealthCheck>("SQS", tags: new[] { "monitoring" });

            services.AddSingleton<ElastiCacheHealthCheck>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var host = configuration["ElastiCache:Host"] ?? "default-host";
                var port = int.TryParse(configuration["ElastiCache:Port"], out var p) ? p : 6379;
                var password = configuration["ElastiCache:Token"] ?? string.Empty;
                return new ElastiCacheHealthCheck(host, port, password);
            });

            services.AddSingleton<SqsHealthCheck>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var sqsClient = provider.GetRequiredService<IAmazonSQS>();

                var queueUrls = new Dictionary<string, string>
                {
                    { "Entity Verification", configuration["SQS:entity_verification_url"] },
                    { "Organisation", configuration["SQS:organisation_url"] }
                };

                return new SqsHealthCheck(sqsClient, queueUrls);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions()
                {
                    Predicate = _ => false, // Exclude all checks to keep this a "service availability" endpoint
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";
                        var response = new { status = report.Status.ToString() };
                        await context.Response.WriteAsJsonAsync(response);
                    }
                });
            });
        }
    }
}
