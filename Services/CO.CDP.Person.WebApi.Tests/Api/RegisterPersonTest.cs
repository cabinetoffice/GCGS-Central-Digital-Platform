using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.Tests.Api.WebApp;
using CO.CDP.Person.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Person.WebApi.Tests.Api;
public class RegisterPersonTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<RegisterPerson, Model.Person>> _registerPersonUseCase = new();

    public RegisterPersonTest()
    {
        TestWebApplicationFactory<Program> factory = new(services =>
        {
            services.AddScoped<IUseCase<RegisterPerson, Model.Person>>(_ => _registerPersonUseCase.Object);
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ItRegistersNewPerson()
    {
        var command = GivenRegisterPersonCommand();
        var person = new Model.Person
        {
            Id = Guid.NewGuid(),
            FirstName = "ThePerson",
            LastName = "lastname",
            Email = "email@email.com",
        };

        _registerPersonUseCase.Setup(useCase => useCase.Execute(It.IsAny<RegisterPerson>()))
                                    .ReturnsAsync(person);

        var response = await _httpClient.PostAsJsonAsync("/persons", command);

        response.Should().HaveStatusCode(Created, await response.Content.ReadAsStringAsync());

        var returnedPerson = await response.Content.ReadFromJsonAsync<Model.Person>();
        returnedPerson.Should().BeEquivalentTo(person, options => options.ComparingByMembers<Model.Person>());
    }

    [Fact]
    public async Task ItHandlesPersonCreationFailure()
    {
        var command = GivenRegisterPersonCommand();

        _registerPersonUseCase.Setup(useCase => useCase.Execute(It.IsAny<RegisterPerson>()))
                                    .ReturnsAsync((Model.Person)null!);

        var response = await _httpClient.PostAsJsonAsync("/persons", command);

        response.Should().HaveStatusCode(HttpStatusCode.InternalServerError, await response.Content.ReadAsStringAsync());
    }

    private static RegisterPerson GivenRegisterPersonCommand()
    {
        return new RegisterPerson
        {
            FirstName = "ThePerson",
            LastName = "lastname",
            Email = "email@email.com",
        };
    }
}