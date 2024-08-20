using System.Reflection;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.EntityVerification.Api;
using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Extensions;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Ppon;
using CO.CDP.MQ;
using CO.CDP.MQ.Hosting;
using CO.CDP.EntityVerification.UseCase;

using Microsoft.EntityFrameworkCore;
using CO.CDP.EntityVerification.Model;
using CO.CDP.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.DocumentPponApi(builder.Configuration));

builder.Services.AddHealthChecks();
builder.Services.AddEntityVerificationProblemDetails();
builder.Services.AddDbContext<EntityVerificationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("EvDatabase")));
builder.Services.AddScoped<IPponRepository, DatabasePponRepository>();
builder.Services.AddScoped<IPponService, PponService>();
builder.Services.AddScoped<IUseCase<LookupIdentifierQuery, IEnumerable<CO.CDP.EntityVerification.Model.Identifier>>, LookupIdentifierUseCase>();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.EntityVerification"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddAwsSqsService()
        .AddSqsPublisher()
        .AddSqsDispatcher(
            EventDeserializer.Deserializer,
            services =>
            {
                services.AddScoped<ISubscriber<OrganisationRegistered>, OrganisationRegisteredSubscriber>();
                services.AddScoped<ISubscriber<OrganisationUpdated>, OrganisationUpdatedSubscriber>();
            },
            (services, dispatcher) =>
            {
                dispatcher.Subscribe<OrganisationRegistered>(services);
                dispatcher.Subscribe<OrganisationUpdated>(services);
            }
        );
    builder.Services.AddHostedService<DispatcherBackgroundService>();
}

builder.Services
    .AddAuthentication()
    .AddJwtBearerAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddEntityVerificationAuthorization();

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
app.MapHealthChecks("/health").AllowAnonymous();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UsePponEndpoints();
app.Run();

public abstract partial class Program;