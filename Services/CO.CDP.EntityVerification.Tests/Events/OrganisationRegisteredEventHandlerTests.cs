using Moq;
using Xunit;
using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;

namespace CO.CDP.EntityVerification.Tests;

public class OrganisationRegisteredEventHandlerTests
{
    [Fact]
    public void Action_GeneratesPponIdAndPersists_CallsGeneratePponIdAndSaveOnce()
    {
        var pponService = new Mock<IPponService>();
        var serviceProvider = new Mock<IServiceProvider>();
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        var serviceScope = new Mock<IServiceScope>();
        var pponRepository = new Mock<IPponRepository>();
        var generatedPpon = "92be415e5985421087bc8fee8c97d338";

        serviceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory.Object);
        serviceScope.Setup(x => x.ServiceProvider.GetService(typeof(IPponRepository))).Returns(pponRepository.Object);
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);
        pponService.Setup(x => x.GeneratePponId()).Returns(generatedPpon);

        var handler = new OrganisationRegisteredEventHandler(pponService.Object, serviceProvider.Object);
        var message = new OrganisationRegisteredMessage
        {
            Name = "MyOrg",
        };

        handler.Action(message);

        pponRepository.Verify(s => s.Save(It.Is<Ppon>(p => p.PponId == generatedPpon)), Times.Once);
    }
}