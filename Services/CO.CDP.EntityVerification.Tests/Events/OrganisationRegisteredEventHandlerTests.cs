using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Services;
using CO.CDP.TestKit.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace CO.CDP.EntityVerification.Tests.Events;

public class OrganisationRegisteredEventHandlerTests
{
    [Fact]
    public void Action_GeneratesPponIdAndPersists()
    {
        var pponRepository = new Mock<IPponRepository>();
        var pponService = new Mock<IPponService>();
        var services = GivenServices(s => s.AddScoped<IPponRepository>(_ => pponRepository.Object));
        var generatedPpon = "92be415e5985421087bc8fee8c97d338";

        pponService.Setup(x => x.GeneratePponId()).Returns(generatedPpon);

        var handler = new OrganisationRegisteredEventHandler(pponService.Object, services);
        var message = new OrganisationRegisteredMessage
        {
            Name = "MyOrg",
        };

        handler.Action(message);

        pponRepository.Verify(s => s.Save(It.Is<Ppon>(p => p.PponId == generatedPpon)), Times.Once);
    }

    private IServiceProvider GivenServices(Action<IServiceCollection> configurator)
    {
        return new TestWebApplicationFactory<Program>(builder => builder.ConfigureServices(configurator)).Services;
    }
}