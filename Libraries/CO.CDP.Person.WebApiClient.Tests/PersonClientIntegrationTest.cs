using FluentAssertions;

namespace CO.CDP.Person.WebApiClient.Tests;

public class PersonClientIntegrationTest
{
    [Fact(Skip = "The test requires the person service to run.")]
    public async Task ItTalksToThePersonApi()
    {
        IPersonClient client = new PersonClient("http://localhost:8084", new HttpClient());

        var newPerson = new NewPerson(
            age: 40,
            email: $"test{DateTime.Now.ToString("ddMMyyyyHHmmssfff")}@email.com",
            firstName: "Test",
            lastName: "ln"
        );

        var person = await client.CreatePersonAsync(newPerson);
        person.Should().NotBeNull();
    }
}