using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.UI.Foundation;
using GovUk.Frontend.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddGovUkFrontend();

builder.Services.AddUiFoundation(builder.Configuration, uiFoundationBuilder =>
{
    uiFoundationBuilder.AddSession(builder.Environment.IsDevelopment());
    uiFoundationBuilder.AddFtsUrlService();
    uiFoundationBuilder.AddCookiePreferences("ROCT");
});

builder.Services.AddHttpClient<ICommercialToolsApiClient, CommercialToolsApiClient>((client) =>
{
    var url = builder.Configuration.GetValue<string>("CommercialToolsApi:ServiceUrl")
              ?? throw new Exception("Missing CommercialToolsApi:ServiceUrl configuration.");
    client.BaseAddress = new Uri(url);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseGovUkFrontend();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();