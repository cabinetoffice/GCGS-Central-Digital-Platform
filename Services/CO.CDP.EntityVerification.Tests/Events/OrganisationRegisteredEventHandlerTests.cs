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
    public void Action_GeneratesPponId_CallsGeneratePponIdWithCorrectParameters()
    {
        // Arrange
        var mockPponService = new Mock<IPponService>();
        var serviceProvider = new Mock<IServiceProvider>(); 
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        var serviceScope = new Mock<IServiceScope>();
        var serviceProviderWrapper = new Mock<IServiceProviderWrapper>();
        var context = new Mock<EntityValidationContext>();

        serviceProviderWrapper.Setup(m => m.GetRequiredService(It.IsAny<IServiceProvider>())).Returns(context.Object);
        serviceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory.Object);
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);
        
        var handler = new OrganisationRegisteredEventHandler(mockPponService.Object, serviceProvider.Object, serviceProviderWrapper.Object);
        var message = new OrganisationRegisteredMessage
        {
            Scheme = "TestScheme",
            GovIdentifier = "TestGovId",
            Name = "MyOrg",
        };

        // Act
        handler.Action(message);

        // Assert
        mockPponService.Verify(s => s.GeneratePponId("TestScheme", "TestGovId"), Times.Once);
        context.Verify(s => s.SaveChanges(), Times.Once);
    }
}