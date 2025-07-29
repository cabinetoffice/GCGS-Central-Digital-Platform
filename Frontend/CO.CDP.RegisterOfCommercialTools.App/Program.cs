using CO.CDP.RegisterOfCommercialTools.App;
using CO.CDP.RegisterOfCommercialTools.App.Middleware;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.UI.Foundation;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using ISession = CO.CDP.RegisterOfCommercialTools.App.ISession;
using Microsoft.FeatureManagement;
using CO.CDP.RegisterOfCommercialTools.App.Constants;
using CO.CDP.UI.Foundation.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFeatureManagement();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/Logout");
});
builder.Services.AddGovUkFrontend();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<ISession, Session>();

builder.Services.AddCors();

var cookieSettings = new CookieSettings();
builder.Configuration.GetSection("CookieSettings").Bind(cookieSettings);
builder.Services.AddSingleton(cookieSettings);
builder.Services.AddScoped<ICookiePreferencesService, CookiePreferencesService>();

builder.Services.AddUiFoundation(builder.Configuration, uiFoundationBuilder =>
{

    uiFoundationBuilder.AddFtsUrlService();
    uiFoundationBuilder.AddSirsiUrlService();
    uiFoundationBuilder.AddCommercialToolsUrlService();

});

builder.Services.AddScoped<CO.CDP.RegisterOfCommercialTools.App.Handlers.BearerTokenHandler>();

builder.Services.AddHttpClient<ISearchService, CommercialToolsApiClient>(client =>
{
    var url = builder.Configuration.GetValue<string>("CommercialToolsApi:ServiceUrl")
              ?? throw new Exception("Missing CommercialToolsApi:ServiceUrl configuration.");
    client.BaseAddress = new Uri(url);
})
.AddHttpMessageHandler<CO.CDP.RegisterOfCommercialTools.App.Handlers.BearerTokenHandler>();

var sessionTimeoutInMinutes = builder.Configuration.GetValue<double>("SessionTimeoutInMinutes", 30);
var cookieSecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeoutInMinutes);
    options.Cookie.Name = "CommercialTools.Session";
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

builder.Services.AddCookiePolicy(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = cookieSecurePolicy;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365); // see https://aka.ms/aspnetcore-hsts
});

var dataProtectionBuilder = builder.Services.AddDataProtection()
    .SetApplicationName("CO.CDP.RegisterOfCommercialTools.App");

var dataProtectionPrefix = builder.Configuration.GetValue<string>("Aws:SystemManager:DataProtectionPrefix");
if (!string.IsNullOrEmpty(dataProtectionPrefix))
{
    dataProtectionBuilder.PersistKeysToAWSSystemsManager(dataProtectionPrefix);
}

builder.Services.AddAwsCognitoAuthentication(builder.Configuration, builder.Environment);

builder.Configuration.GetValue<string>("CommercialToolsApi:ServiceUrl");
builder.Services.AddScoped<CO.CDP.UI.Foundation.Pages.NotFoundPage>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var featureManager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();
    if (!await featureManager.IsEnabledAsync(FeatureFlags.CommercialTools))
    {
        app.UseStatusCodePages(async context =>
        {
            context.HttpContext.Response.ContentType = "text/plain";
            context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.HttpContext.Response.WriteAsync("404 Not Found");
        });
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseMiddleware<ContentSecurityPolicyMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseAntiforgery();
}

app.UseCookiePolicy();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAllOrigins"); // to be updated when OneLogin is implemented

app.UseGovUkFrontend();

app.UseAuthentication();
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.MapFallback(ctx =>
{
    ctx.Response.Redirect("/page-not-found");
    return Task.CompletedTask;
});

app.Run();
