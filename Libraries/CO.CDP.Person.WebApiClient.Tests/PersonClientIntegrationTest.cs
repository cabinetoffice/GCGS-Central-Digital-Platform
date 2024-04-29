namespace CO.CDP.Person.WebApiClient.Tests;

public class PersonClientIntegrationTest
{
    [Fact(Skip = "The test requires the person service to run.")]
    public async Task ItTalksToTheOrganisationApi()
    {
        IPersonClient client = new PersonClient("http://localhost:5120", new HttpClient());

        var newPerson = new RegisterPerson(        
            age: 40,
            email: "test@email.com",
            name: "Test"
        );

        var person = await client.CreatePersonAsync(newPerson);

        var foundPerson = await client.GetPersonsAsync(person.Id);

        Assert.Equal(person, foundPerson);
    }
}