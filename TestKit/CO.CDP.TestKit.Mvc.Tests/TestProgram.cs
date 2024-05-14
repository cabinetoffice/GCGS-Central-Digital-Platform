using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<Func<String>>(_ => () => Random.Shared.Next().ToString());
var app = builder.Build();
app.MapGet("/hello", (Func<String> names) => $"Hello, {names()}!");
app.Run();

public abstract partial class TestProgram;
