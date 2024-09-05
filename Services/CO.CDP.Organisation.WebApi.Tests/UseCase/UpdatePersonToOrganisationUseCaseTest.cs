using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdatePersonToOrganisationUseCaseTest : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly UpdatePersonToOrganisationUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();

    public UpdatePersonToOrganisationUseCaseTest(AutoMapperFixture mapperFixture)
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _useCase = new UpdatePersonToOrganisationUseCase(_organisationRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldUpdatePerson_When_Organisation_Person_Exists()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation
        {
            Scopes = ["ADMIN","EDITOR"]
        };
        var OrganisationPerson = organisationPerson;
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId)).ReturnsAsync(OrganisationPerson);

        var result = await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));       

        result.Should().Be(true);
        _organisationRepositoryMock.Verify(repo => repo.SaveOrganisationPerson(OrganisationPerson!), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowEmptyPersonRoleException_WhenPersonScopeIsEmpty()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation
        {
            Scopes = []
        };
        var OrganisationPerson = organisationPerson;
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId)).ReturnsAsync(OrganisationPerson);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        await act.Should()
            .ThrowAsync<EmptyPersonRoleException>()
            .WithMessage($"Empty Scope of Person {_personId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownPersonException_WhenPersonOrOrganisationNotFound()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation
        {
            Scopes = ["Viewer"]
        };
        var OrganisationPerson = organisationPerson;
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId)).ReturnsAsync((OrganisationPerson?)null);


        Func<Task> act = async () => await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        await act.Should()
        .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId} or Person {_personId}.");
    }

  
    private Persistence.Organisation Organisation =>
        new Persistence.Organisation
        {
            Guid = _organisationId,
            Name = "Test",
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.Organisation.SupplierInformation()
        };

    private Persistence.Person person =>
       new Persistence.Person
       {
           Guid = _personId,
           FirstName = "Test",
           LastName = "Test",
           Email = "Test@test.com"
       };

    private Persistence.OrganisationPerson organisationPerson =>
     new Persistence.OrganisationPerson
     {
         Organisation = Organisation,
         OrganisationId = Organisation.Id,
         PersonId = person.Id,
         Person = person,
         Scopes = ["Viewer"]
     };
}