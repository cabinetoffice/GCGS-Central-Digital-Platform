using CO.CDP.Configuration.Helpers;
using CO.CDP.GovUKNotify;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.ScheduledWorker;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());
builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());
builder.Services.AddDbContext<OrganisationInformationContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddScoped<IMouRepository, DatabaseMouRepository>();
builder.Services.AddHostedService<CompleteMoUTimedHostedService>();
builder.Services.AddScoped<IScopedProcessingService, CompleteMoUReminderService>();
builder.Services.AddGovUKNotifyApiClient(builder.Configuration);

var app = builder.Build();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();