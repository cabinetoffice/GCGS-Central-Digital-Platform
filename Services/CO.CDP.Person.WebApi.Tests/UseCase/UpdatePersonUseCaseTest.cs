using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace CO.CDP.Person.WebApi.Tests.UseCase;

public class UpdatePersonUseCaseTests
{
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly UpdatePersonUseCase _useCase;

    public UpdatePersonUseCaseTests()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _useCase = new UpdatePersonUseCase(_personRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_WhenPersonExists_ShouldUpdateUserUrnAndSave()
    {
        var personId = Guid.NewGuid();
        var existingPerson = new OrganisationInformation.Persistence.Person {
            FirstName = "Tom",
            LastName = "Smith",
            Email = "email@test.com",
            Guid = Guid.NewGuid(),
            UserUrn = "oldUrn"
        };
        var updatePerson = new UpdatePerson { UserUrn = "newUrn" };

        _personRepositoryMock.Setup(repo => repo.Find(personId))
            .ReturnsAsync(existingPerson);

        var result = await _useCase.Execute((personId, updatePerson));

        result.Should().BeTrue();
        existingPerson.PreviousUrns.Should().Contain("oldUrn");
        existingPerson.UserUrn.Should().Be("newUrn");
        _personRepositoryMock.Verify(repo => repo.Save(existingPerson), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenPersonDoesNotExist_ShouldThrowUnknownPersonException()
    {
        var personId = Guid.NewGuid();
        var updatePerson = new UpdatePerson { UserUrn = "newUrn" };

        _personRepositoryMock.Setup(repo => repo.Find(personId))
            .ReturnsAsync((OrganisationInformation.Persistence.Person?)null);

        var act = async () => await _useCase.Execute((personId, updatePerson));
        await act.Should().ThrowAsync<UnknownPersonException>();
    }
}