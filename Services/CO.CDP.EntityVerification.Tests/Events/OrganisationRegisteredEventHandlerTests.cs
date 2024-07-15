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
        // Arrange
        var mockPponService = new Mock<IPponService>();
        var serviceProvider = new Mock<IServiceProvider>();
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        var serviceScope = new Mock<IServiceScope>();
        var context = new Mock<EntityVerificationContext>();
        var pponRepository = new Mock<IPponRepository>();

        serviceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory.Object);
        serviceScope.Setup(x => x.ServiceProvider.GetService(typeof(EntityVerificationContext)))
            .Returns(context.Object);
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

        var handler = new OrganisationRegisteredEventHandler(mockPponService.Object, pponRepository.Object, serviceProvider.Object);
        var message = new OrganisationRegisteredMessage
        {
            Name = "MyOrg",
        };

        // Act
        handler.Action(message);

        // Assert
        mockPponService.Verify(s => s.GeneratePponId(), Times.Once);
        pponRepository.Verify(s => s.Save(It.IsAny<EntityVerificationContext>(), It.IsAny<Ppon>()), Times.Once);
    }
}