using FluentAssertions;

namespace CO.CDP.Person.WebApiClient.Tests;

public class PersonClientIntegrationTest
{
    [Fact(Skip = "The test requires the person service to run.")]
    public async Task ItTalksToThePersonApi()
    {
        IPersonClient client = new PersonClient("http://localhost:8084", new HttpClient());

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