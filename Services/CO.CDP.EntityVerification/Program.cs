using System.Reflection;
using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.EntityVerification;
using CO.CDP.EntityVerification.Api;
using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Ppon;
using CO.CDP.EntityVerification.UseCase;
using CO.CDP.MQ;
using CO.CDP.WebApi.Foundation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Identifier = CO.CDP.EntityVerification.Model.Identifier;
using IdentifierRegistries = CO.CDP.EntityVerification.Model.IdentifierRegistries;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.DocumentPponApi(builder.Configuration));

builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(_ => new NpgsqlDataSourceBuilder(ConnectionStringHelper.GetConnectionString(builder.Configuration, "EntityVerificationDatabase")).Build());
builder.Services.AddDbContext<EntityVerificationContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
builder.Services.AddScoped<IPponRepository, DatabasePponRepository>();
builder.Services.AddScoped<IPponService, PponService>();

builder.Services.AddScoped<IUseCase<LookupIdentifierQuery, IEnumerable<Identifier>>, LookupIdentifierUseCase>();
builder.Services.AddScoped<IUseCase<string, IEnumerable<IdentifierRegistries>>, GetIdentifierRegistriesUseCase>();
builder.Services.AddScoped<IUseCase<string[], IEnumerable<IdentifierRegistries>>, GetIdentifierRegistriesDetailsUseCase>();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.EntityVerification"))
{
    builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());

    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration)
        .AddAwsSqsService()
        .AddOutboxSqsPublisher<EntityVerificationContext>(
            builder.Configuration,
            enableBackgroundServices: Assembly.GetEntryAssembly().IsRunAs("CO.CDP.EntityVerification"),
            notificationChannel: "entity_verification_outbox")
        .AddSqsDispatcher(
            EventDeserializer.Deserializer,
            enableBackgroundServices: Assembly.GetEntryAssembly().IsRunAs("CO.CDP.EntityVerification"),
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
}

builder.Services
    .AddAuthentication()
    .AddJwtBearerAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddEntityVerificationAuthorization();

var app = builder.Build();
app.UseForwardedHeaders();
app.UseErrorHandler(ErrorCodes.Exception4xxMap);

// Configure the HTTP request pipeline.
if (builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePages();
app.MapHealthChecks("/health").AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();
app.UsePponEndpoints();
app.UseRegistriesEndpoints();

app.Run();

public abstract partial class Program;
