using CO.CDP.Localization;
using System.Resources;

namespace CO.CDP.Mvc.Validation.Tests;

public class ErrorMessageResolverTests
{
    [Fact]
    public void GetErrorMessage_ReturnsErrorMessage_WhenDirectMessageIsProvided()
    {
        var errorMessage = "Direct error message";
        var result = ErrorMessageResolver.GetErrorMessage(errorMessage, null, null);

        Assert.Equal(errorMessage, result);
    }

    [Fact]
    public void GetErrorMessage_ReturnsResourceMessage_WhenResourceNameAndTypeAreValid()
    {
        var errorMessageResourceName = "Global_Yes";
        var errorMessageResourceType = typeof(StaticTextResource);

        var result = ErrorMessageResolver.GetErrorMessage(null, errorMessageResourceName, errorMessageResourceType);

        Assert.Equal("Yes", result);
    }

    [Fact]
    public void GetErrorMessage_ThrowsException_WhenResourceManagerNotFound()
    {
        var errorMessageResourceName = "InvalidKey";
        var errorMessageResourceType = typeof(NoResourceManager);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ErrorMessageResolver.GetErrorMessage(null, errorMessageResourceName, errorMessageResourceType));

        Assert.Equal("No ResourceManager found in 'CO.CDP.Mvc.Validation.Tests.NoResourceManager'.", exception.Message);
    }

    [Fact]
    public void GetErrorMessage_ThrowsException_WhenResourceManagerPropertyIsNull()
    {
        var errorMessageResourceName = "SomeKey";
        var errorMessageResourceType = typeof(ResourceManagerWithNull);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ErrorMessageResolver.GetErrorMessage(null, errorMessageResourceName, errorMessageResourceType));

        Assert.Equal("No ResourceManager found in 'CO.CDP.Mvc.Validation.Tests.ResourceManagerWithNull'.", exception.Message);
    }

    [Fact]
    public void GetErrorMessage_ReturnsNull_WhenErrorMessageAndResourceAreUnavailable()
    {
        var result = ErrorMessageResolver.GetErrorMessage(null, null, null);
        Assert.Null(result);
    }

    [Fact]
    public void GetErrorMessage_ReturnsNull_WhenResourceKeyDoesNotExist()
    {
        var errorMessageResourceName = "KeyThatDefinitelyDoesntExist";
        var errorMessageResourceType = typeof(StaticTextResource);

        var result = ErrorMessageResolver.GetErrorMessage(null, errorMessageResourceName, errorMessageResourceType);

        Assert.Null(result);
    }
}

internal class NoResourceManager
{
}

internal class ResourceManagerWithNull
{
    public static ResourceManager? ResourceManager => null;
}