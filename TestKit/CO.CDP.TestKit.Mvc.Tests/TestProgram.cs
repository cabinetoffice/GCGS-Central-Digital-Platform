using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<Func<String>>(_ => () => Random.Shared.Next().ToString());
builder.Services.AddDbContext<DbContext>();
var app = builder.Build();
app.MapGet("/hello", (Func<String> names) => $"Hello, {names()}!");
app.MapGet("/log", (ILogger<string> logger) =>
{
    logger.Log(LogLevel.Information, "Hello!");
    return "Hello!";
});
app.MapGet("/db", (DbContext dbContext) => dbContext.Database.ProviderName);
app.Run();

public abstract partial class TestProgram;