using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Tests.Persistence;
using CO.CDP.EntityVerification.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Moq;
using static CO.CDP.EntityVerification.UseCase.GetIdentifierRegistriesUseCase;
using static CO.CDP.EntityVerification.UseCase.GetIdentifierRegistriesUseCase.GetIdentifierRegistriesException;

namespace CO.CDP.EntityVerification.Tests.UseCase;

using Moq;
using Xunit;

public class GetIdentifierRegistriesDetailsUseCaseTest
{
    private readonly Mock<IPponRepository> _repository;
    private readonly GetIdentifierRegistriesDetailsUseCase _useCase;

    public GetIdentifierRegistriesDetailsUseCaseTest()
    {
        _repository = new Mock<IPponRepository>();

        _useCase = new GetIdentifierRegistriesDetailsUseCase(_repository.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnIdentifiers_WhenRepositoryReturnsData()
    {        
        var schemecodes = new string[] { "scheme1", "scheme2" };
        var rawIdentifiers = new[]
        {
                new IdentifierRegistries { Id = 1, CountryCode = "US", Scheme = "scheme1", RegisterName = "Register1", CreatedOn = DateTimeOffset.Now, UpdatedOn = DateTimeOffset.Now },
                new IdentifierRegistries { Id = 2, CountryCode = "FR", Scheme = "scheme2", RegisterName = "Register2", CreatedOn = DateTimeOffset.Now, UpdatedOn = DateTimeOffset.Now }
            };

        _repository
            .Setup(repo => repo.GetIdentifierRegistriesNameAsync(schemecodes))
            .ReturnsAsync(rawIdentifiers);

        var result = await _useCase.Execute(schemecodes);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result.ElementAt(0).Countrycode.Should().Be("US");
        result.ElementAt(0).Scheme.Should().Be("scheme1");
        result.ElementAt(0).RegisterName.Should().Be("Register1");

        result.ElementAt(1).Countrycode.Should().Be("FR");
        result.ElementAt(1).Scheme.Should().Be("scheme2");
        result.ElementAt(1).RegisterName.Should().Be("Register2");
    }

    [Fact]
    public async Task Execute_ShouldReturnEmpty_WhenRepositoryReturnsNoData()
    {
        var schemecodes = new string[] { "scheme1", "scheme2" };
        _repository
            .Setup(repo => repo.GetIdentifierRegistriesNameAsync(schemecodes))
            .ReturnsAsync(Enumerable.Empty<IdentifierRegistries>());

        var result = await _useCase.Execute(schemecodes);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenRepositoryThrowsException()
    {
        var schemecodes = new string[] { "scheme1", "scheme2" };
        _repository
            .Setup(repo => repo.GetIdentifierRegistriesNameAsync(schemecodes))
            .ThrowsAsync(new System.Exception("Database error"));

        await Assert.ThrowsAsync<System.Exception>(() => _useCase.Execute(schemecodes));
    }

}