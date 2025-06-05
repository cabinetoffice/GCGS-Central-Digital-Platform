using AutoMapper;
using CO.CDP.DataSharing.WebApi.AutoMapper;
using CO.CDP.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.DataSharing.WebApi.Tests.AutoMapper;

public class AutoMapperFixture
{
    public readonly MapperConfiguration Configuration = new(
        config => config.AddProfile<DataSharingProfile>()
    );

    public IMapper Mapper { get; }

    public AutoMapperFixture()
    {
        var localizerMock = new Mock<IHtmlLocalizer<FormsEngineResource>>();

        localizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) =>
            {
                if (key == "Localized_String")
                {
                    return new LocalizedHtmlString("Localized_String", "Localized string");
                }

                return new LocalizedHtmlString(key, key);
            });


        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("DataSharingApiUrl", "http://example1.com/"),
                ])
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IHtmlLocalizer<FormsEngineResource>>(localizerMock.Object);
        services.AddTransient(typeof(NullableLocalizedPropertyResolver<,>));
        services.AddTransient(typeof(LocalizedPropertyResolver<,>));
        services.AddTransient(typeof(FormQuestionOptionsResolver));
        services.AddTransient(typeof(DocumentUriValueResolver));
        services.AddSingleton<IConfiguration>(configuration);

        var serviceProvider = services.BuildServiceProvider();

        Mapper = Configuration.CreateMapper(type => serviceProvider.GetService(type) ?? Activator.CreateInstance(type));
    }
}
