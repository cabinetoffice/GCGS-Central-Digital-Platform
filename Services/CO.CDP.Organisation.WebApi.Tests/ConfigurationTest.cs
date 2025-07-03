using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.Organisation.WebApi.Tests;

public class ConfigurationTest
{
    [Fact]
    public void ItLoadsProductionConfiguration()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("AwsProduction");
            });

        var buyerParentChildRelationship = application.Services.GetRequiredService<IConfiguration>().GetValue<bool?>("BuyerParentChildRelationship");

        buyerParentChildRelationship.Should().BeFalse();
    }
}

