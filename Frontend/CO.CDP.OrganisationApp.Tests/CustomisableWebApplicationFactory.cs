using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class CustomisableWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly IServiceCollection _replacementServices;

    public CustomisableWebApplicationFactory(IServiceCollection replacementServices)
    {
        _replacementServices = replacementServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(async services =>
        {
            if (_replacementServices != null)
            {
                foreach (var replacementService in _replacementServices)
                {
                    var serviceDescriptorForServiceToReplace = services.FirstOrDefault(descriptor => descriptor.ServiceType == replacementService.ServiceType);
                    if (serviceDescriptorForServiceToReplace != null)
                    {
                        services.Remove(serviceDescriptorForServiceToReplace);
                        services.Add(replacementService);
                    }
                }
            }
        });
    }
}