using CO.CDP.AwsServices;
using CO.CDP.EntityVerification.Api;
using CO.CDP.EntityVerification.Extensions;
using CO.CDP.EntityVerification.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.DocumentPponApi());

builder.Services.AddHealthChecks();
builder.Services.AddEntityVerificationProblemDetails();
builder.Services
    .AddAwsCofiguration(builder.Configuration)
    .AddAwsSqsService();
builder.Services.AddBackgroundServices(builder.Configuration);

builder.Services.AddDbContext<EntityVerificationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("EvDatabase")));

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseStatusCodePages();
app.MapHealthChecks("/health").AllowAnonymous();
app.UseHttpsRedirection();
app.UsePponEndpoints();
app.Run();

public abstract partial class Program;