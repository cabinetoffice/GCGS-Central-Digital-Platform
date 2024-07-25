using System.Reflection;
using Amazon.SQS;
using CO.CDP.AwsServices;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.Configuration.Assembly;
using CO.CDP.EntityVerification.Api;
using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Extensions;
using CO.CDP.EntityVerification.MQ;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Ppon;
using CO.CDP.MQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.DocumentPponApi());

builder.Services.AddHealthChecks();
builder.Services.AddEntityVerificationProblemDetails();
builder.Services.AddDbContext<EntityVerificationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("EvDatabase")));
builder.Services.AddScoped<IPponRepository, DatabasePponRepository>();
builder.Services.AddScoped<IPponService, PponService>();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.EntityVerification"))
{
    builder.Services
        .AddAwsCofiguration(builder.Configuration)
        .AddAwsSqsService()
        .AddSqsPublisher();
    builder.Services.AddScoped<IEventHandler<OrganisationRegistered>, OrganisationRegisteredEventHandler>();
    builder.Services.AddScoped<Deserializer>(_ => EventDeserializer.Deserializer);
    builder.Services.AddScoped<IDispatcher, SqsDispatcher>(s =>
    {
        var dispatcher = new SqsDispatcher(
            s.GetRequiredService<IAmazonSQS>(),
            s.GetRequiredService<IOptions<AwsConfiguration>>(),
            s.GetRequiredService<Deserializer>());
        dispatcher.Subscribe<OrganisationRegistered>(s.GetRequiredService<IEventHandler<OrganisationRegistered>>()
            .Handle);
        return dispatcher;
    });
    builder.Services.AddHostedService<QueueBackgroundService>();
}

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