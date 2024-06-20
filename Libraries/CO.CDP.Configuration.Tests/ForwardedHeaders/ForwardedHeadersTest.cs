using CO.CDP.Configuration.ForwardedHeaders;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders;
using ForwardedHeadersOptions = Microsoft.AspNetCore.Builder.ForwardedHeadersOptions;

namespace CO.CDP.Configuration.Tests.ForwardedHeaders;

public class ForwardedHeadersTest
{
    private readonly IPNetwork _defaultNetwork = IPNetwork.Parse("127.0.0.1/8");

    [Fact]
    public void ItDoesNotRegisterForwardedHeadersOptionsByDefault()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        var options = app.Services.GetService<IOptions<ForwardedHeadersOptions>>()
            .As<IOptions<ForwardedHeadersOptions>>();

        options.Value.ForwardedHeaders.Should()
            .Be(None);
        options.Value.KnownNetworks.Should()
            .ContainEquivalentOf(_defaultNetwork);
    }

    [Fact]
    public void ItRegistersForwardedHeadersOptions()
    {
        var builder = WebApplication.CreateBuilder();
        builder.ConfigureForwardedHeaders();
        var app = builder.Build();

        var options = app.Services.GetService<IOptions<ForwardedHeadersOptions>>()
            .As<IOptions<ForwardedHeadersOptions>>();

        options.Value.ForwardedHeaders.Should()
            .Be(XForwardedFor | XForwardedProto);
        options.Value.KnownNetworks.Should()
            .ContainEquivalentOf(_defaultNetwork);
    }

    [Fact]
    public void ItConfiguresKnownNetwork()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "ForwardedHeaders:KnownNetwork", "10.3.0.0/24" },
        }!);
        builder.ConfigureForwardedHeaders();
        var app = builder.Build();

        var options = app.Services.GetService<IOptions<ForwardedHeadersOptions>>()
            .As<IOptions<ForwardedHeadersOptions>>();

        options.Value.ForwardedHeaders.Should()
            .Be(XForwardedFor | XForwardedProto);
        options.Value.KnownNetworks.Should()
            .ContainEquivalentOf(_defaultNetwork);
        options.Value.KnownNetworks.Should()
            .ContainEquivalentOf(IPNetwork.Parse("10.3.0.0/24"));
    }
}