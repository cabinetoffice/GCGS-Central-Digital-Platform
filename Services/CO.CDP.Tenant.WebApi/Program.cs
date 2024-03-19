using CO.CDP.Tenant.Persistence;
using CO.CDP.Tenant.WebApi.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentTenantApi(); });

builder.Services.AddHealthChecks();

builder.Services.AddScoped<TenantContext>(_ => new TenantContext(builder.Configuration.GetConnectionString("TenantDatabase") ?? ""));
builder.Services.AddScoped<ITenantRepository, DatabaseTenantRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseTenantEndpoints();
app.UseTenantLookupEndpoints();
app.UseUserManagementEndpoints();
app.Run();

public abstract partial class Program
{
}