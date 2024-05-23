using CO.CDP.Swashbuckle.Security;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using FluentAssertions;

namespace CO.CDP.Swashbuckle.Tests.Security;

public class SecurityExtensionsTest
{
    [Fact]
    public void ItConfiguresBearerAuthentication()
    {
        var options = new SwaggerGenOptions();

        options.ConfigureOneLoginSecurity();

        options.SwaggerGeneratorOptions.SecuritySchemes
            .Should().Contain(
                s => s.Key == "OneLogin" && s.Value.Scheme == "Bearer");
        options.SwaggerGeneratorOptions.SecurityRequirements.First()
            .Should().Contain(r => r.Key.Reference.Id == "OneLogin");
    }

    [Fact]
    public void ItConfiguresApiKeyAuthentication()
    {
        var options = new SwaggerGenOptions();

        options.ConfigureApiKeySecurity();

        options.SwaggerGeneratorOptions.SecuritySchemes
            .Should().Contain(
                s => s.Key == "ApiKey");
        options.SwaggerGeneratorOptions.SecurityRequirements.First()
            .Should().Contain(r => r.Key.Reference.Id == "ApiKey");
    }
}