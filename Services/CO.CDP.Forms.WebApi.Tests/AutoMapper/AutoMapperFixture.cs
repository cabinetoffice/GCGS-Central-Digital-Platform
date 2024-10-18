using AutoMapper;
using CO.CDP.Forms.WebApi.AutoMapper;
using CO.CDP.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.Forms.WebApi.Tests.AutoMapper;

public class AutoMapperFixture
{
    public readonly MapperConfiguration Configuration = new(
        config => config.AddProfile<WebApiToPersistenceProfile>()
    );

    public IMapper Mapper { get; }

    public AutoMapperFixture()
    {
        var localizerMock = new Mock<IHtmlLocalizer<FormsEngineResource>>();

        localizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) =>
            {
                if (key == "FinancialInformation_SectionTitle")
                {
                    return new LocalizedHtmlString("FinancialInformation_SectionTitle", "Financial Information");
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
