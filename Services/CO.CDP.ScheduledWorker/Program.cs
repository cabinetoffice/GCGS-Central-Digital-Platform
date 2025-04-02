using CO.CDP.Configuration.Helpers;
using CO.CDP.GovUKNotify;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.ScheduledWorker;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());
builder.Services.AddDbContext<OrganisationInformationContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddScoped<IMouRepository, DatabaseMouRepository>();
builder.Services.AddHostedService<CompleteMoUTimedHostedService>();
builder.Services.AddScoped<IScopedProcessingService, CompleteMoUReminderService>();
builder.Services.AddGovUKNotifyApiClient(builder.Configuration);

using IHost host = builder.Build();
await host.RunAsync();