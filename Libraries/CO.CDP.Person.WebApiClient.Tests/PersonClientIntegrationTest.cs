using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Xunit.Abstractions;

namespace CO.CDP.Person.WebApiClient.Tests;

public class PersonClientIntegrationTest(ITestOutputHelper testOutputHelper)
{
    private readonly TestWebApplicationFactory<Program> _factory = new(builder =>
    {
        builder.ConfigureInMemoryDbContext<OrganisationInformationContext>();
        builder.ConfigureLogging(testOutputHelper);
    });

    [Fact]
    public async Task ItTalksToThePersonApi()
    {
        IPersonClient client = new PersonClient("https://localhost", _factory.CreateClient());

        var newPerson = new NewPerson(
            email: $"test{DateTime.Now.ToString("ddMMyyyyHHmmssfff")}@email.com",
            phone: "07925123123",
            firstName: "Test",
            lastName: "ln",
            userUrn: $"urn:fdc:gov.uk:2022:{Guid.NewGuid()}"
        );

        var person = await client.CreatePersonAsync(newPerson);
        person.Should().NotBeNull();
    }
}