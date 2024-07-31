using Microsoft.OpenApi.Models;
using Amazon.SQS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using healthCheck.Models;
using Microsoft.OpenApi.Any;
using YourProject.Middleware; // Ensure the namespace for the middleware is correct

namespace healthCheck
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton<IAmazonSQS>(sp => new AmazonSQSClient());

            services.AddHealthChecks();

            bool enableSwaggerUI = Configuration.GetSection("Features").GetValue<bool>("SwaggerUI");

            // Log the value of the SwaggerUI feature flag
            Console.WriteLine($"SwaggerUI Enabled: {enableSwaggerUI}");

            if (enableSwaggerUI)
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HealthCheck API", Version = "v1" });
                    c.MapType<QueueNames>(() => new OpenApiSchema
                    {
                        Type = "string",
                        Enum = Enum.GetNames(typeof(QueueNames)).Select(name => new OpenApiString(name) as IOpenApiAny).ToList()
                    });
                });
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Log the environment to verify
            Console.WriteLine($"Environment: {env.EnvironmentName}");

            bool enableSwaggerUI = Configuration.GetSection("Features").GetValue<bool>("SwaggerUI");

            // Log the value of the SwaggerUI feature flag
            Console.WriteLine($"SwaggerUI Enabled: {enableSwaggerUI}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            if (enableSwaggerUI)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthCheck API V1");
                    c.RoutePrefix = "swagger";
                });
            }
            else
            {
                app.UseMiddleware<SwaggerDisabledMiddleware>();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }
    }
}
