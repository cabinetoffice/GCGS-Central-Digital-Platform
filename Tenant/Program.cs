using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "1.0.0.0",
        Title = "Tenant Management API",
        Description =
            "API for creating, updating, deleting, and listing tenants, including a lookup feature against person identifiers.",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/tenants", () =>
    {
        return Enumerable.Range(1, 5).Select(index =>
            new Tenant(
                index.ToString(),
                $"Bobby Tables {index}"
            )
        ).ToArray();
    })
    .WithName("listTenants")
    .WithSummary("A list of tenants.")
    .WithDescription("A list of tenants.")
    .WithOpenApi();

app.Run();

internal record Tenant(string Id, string Name)
{
}