using AutoMapper;
using CO.CDP.DataSharing.WebApi.AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.DataSharing.WebApi.Tests.AutoMapper;

public class AutoMapperFixture
{
    private ServiceProvider _serviceProvider;
    public IMapper Mapper { get; private set; }
    public MapperConfiguration Configuration { get; private set; }

    public AutoMapperFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddScoped<IConfigurationService, ConfigurationService>();
        serviceCollection.AddScoped(provider =>
        {
            Configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DataSharingProfile(provider.GetService<IConfigurationService>()));
            });

            return Configuration.CreateMapper();
        });

        _serviceProvider = serviceCollection.BuildServiceProvider();
        Mapper = serviceCollection.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}