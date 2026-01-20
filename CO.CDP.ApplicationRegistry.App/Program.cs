using CO.CDP.ApplicationRegistry.App;
using CO.CDP.ApplicationRegistry.App.Api;
using CO.CDP.ApplicationRegistry.App.Services;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Routing configuration
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.ConstraintMap["snake_case"] = typeof(SnakeCaseParameterTransformer);
});

builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SnakeCaseParameterTransformer()));
});

builder.Services.AddRazorPages();
builder.Services.AddGovUkFrontend();

// API client configuration
var apiBaseUrl = builder.Configuration["ApplicationRegistryApi:BaseUrl"]
                 ?? "http://localhost:7001";

builder.Services
    .AddRefitClient<IApplicationsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services
    .AddRefitClient<IOrganisationsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services
    .AddRefitClient<IOrganisationApplicationsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services
    .AddRefitClient<IUserAssignmentsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl));

// Application services
builder.Services.AddScoped<IApplicationService, ApplicationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseGovUkFrontend();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await app.RunAsync();
