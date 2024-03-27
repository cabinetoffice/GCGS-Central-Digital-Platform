using CO.CDP.OrganisationApp;
using CO.CDP.OrganisationApp.ServiceClient;
using CO.CDP.Tenant.WebApiClient;

const string TenantHttpClientName = "TenantHttpClient";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<double>("SessionTimeoutInMinutes"));
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<CO.CDP.OrganisationApp.ISession, Session>();

builder.Services.AddTransient<IOneLoginClient, FakeOneLoginClient>();

var tenantServiceUrl = builder.Configuration.GetValue<string>("TenantService");

builder.Services.AddHttpClient(TenantHttpClientName);

builder.Services.AddTransient<ITenantClient, TenantClient>(
    sc => new TenantClient(tenantServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(TenantHttpClientName)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.Run();