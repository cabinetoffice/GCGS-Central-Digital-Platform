using AutoMapper;
using CO.CDP.Organisation.WebApi.AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.Organisation.WebApi.Tests.AutoMapper;

public class AutoMapperFixture
{
    private readonly ServiceProvider _serviceProvider;
    public IMapper Mapper { get; private set; }
    public MapperConfiguration? Configuration { get; private set; } = null;

    public AutoMapperFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddScoped<OrganisationInformation.IConfigurationService, ConfigurationService>();
        serviceCollection.AddScoped(provider =>
        {
            Configuration = new MapperConfiguration(cfg =>
            {
                var configurationService = provider.GetService<OrganisationInformation.IConfigurationService>();
                if (configurationService != null)
                {
                    cfg.AddProfile(new WebApiToPersistenceProfile(configurationService));
                }
            });

            return Configuration.CreateMapper();
        });

        _serviceProvider = serviceCollection.BuildServiceProvider();
        Mapper = _serviceProvider.GetRequiredService<IMapper>();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}