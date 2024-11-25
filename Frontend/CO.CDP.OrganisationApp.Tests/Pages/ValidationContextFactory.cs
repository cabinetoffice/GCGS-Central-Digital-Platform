using CO.CDP.OrganisationApp.Pages.Registration;
using Microsoft.Extensions.Localization;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests;

public static class ValidationContextFactory
{
    public static ValidationContext GivenValidationContextWithStringLocalizerFactory(object model, IStringLocalizer stringLocalizer)
    {
        var stringLocalizerFactoryMock = new Mock<IStringLocalizerFactory>();
        stringLocalizerFactoryMock
            .Setup(factory => factory.Create(It.IsAny<Type>()))
            .Returns(stringLocalizer);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(provider => provider.GetService(typeof(IStringLocalizerFactory)))
            .Returns(stringLocalizerFactoryMock.Object);

        serviceProviderMock
            .Setup(provider => provider.GetService(typeof(IServiceProvider)))
            .Returns(serviceProviderMock.Object);

        var validationContext = new ValidationContext(model, serviceProviderMock.Object, null);

        return validationContext;
    }
}