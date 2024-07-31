using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.DataSharing.WebApi.Api;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.DocumentDataSharingApi();
});

builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseStatusCodePages();

app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseDataSharingEndpoints();
app.Run();