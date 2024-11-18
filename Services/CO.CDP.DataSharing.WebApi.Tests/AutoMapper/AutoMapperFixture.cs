using AutoMapper;
using CO.CDP.DataSharing.WebApi.AutoMapper;
using CO.CDP.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
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

        var services = new ServiceCollection();
        services.AddSingleton<IHtmlLocalizer<FormsEngineResource>>(localizerMock.Object);
        services.AddTransient(typeof(NullableLocalizedPropertyResolver<,>));
        services.AddTransient(typeof(LocalizedPropertyResolver<,>));

        var serviceProvider = services.BuildServiceProvider();

        Mapper = Configuration.CreateMapper(type => serviceProvider.GetService(type) ?? Activator.CreateInstance(type));
    }
}
