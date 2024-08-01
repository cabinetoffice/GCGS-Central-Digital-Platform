using Microsoft.OpenApi.Models;
using Amazon.SQS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using healthCheck.Middleware;
using healthCheck.Models;
using Microsoft.OpenApi.Any;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;

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

            if (enableSwaggerUI)
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HealthCheck API", Version = "v1" });
                    c.EnableAnnotations(); // Enable Swagger Annotations
                    c.MapType<QueueNames>(() => new OpenApiSchema
                    {
                        Type = "string",
                        Enum = Enum.GetNames(typeof(QueueNames)).Select(name => new OpenApiString(name) as IOpenApiAny).ToList()
                    });
                    c.SchemaFilter<DefaultSchemaFilter>();
                });
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            bool enableSwaggerUI = Configuration.GetSection("Features").GetValue<bool>("SwaggerUI");

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

            app.Use(async (context, next) =>
            {
                var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
                var frontendUrl = configuration["FrontendUrl"];

                if (context.Request.Path == "/" || context.Request.Path == "/index.html")
                {
                    var indexPath = Path.Combine(env.WebRootPath, "index.html");
                    var html = await File.ReadAllTextAsync(indexPath);
                    html = html.Replace("${frontend_url}", frontendUrl);
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(html);
                }
                else
                {
                    await next();
                }
            });

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

    public class DefaultSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(SendMessageRequestModel))
            {
                schema.Properties["messageGroupId"].Default = new OpenApiString("health-check-test-group");
                schema.Properties["messageGroupId"].Description = "Message group ID for FIFO queues (default: health-check-test-group)";
                schema.Properties["message"].Description = "The content of the message to be sent";
                schema.Properties["queue"].Description = "The name of the queue";
            }
        }
    }
}
