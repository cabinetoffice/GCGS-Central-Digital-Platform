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

public class GetIdentifierRegistriesUseCaseTest
{
    private readonly Mock<IPponRepository> _repository;
    private readonly GetIdentifierRegistriesUseCase _useCase;

    public GetIdentifierRegistriesUseCaseTest()
    {
        _repository = new Mock<IPponRepository>();

        _useCase = new GetIdentifierRegistriesUseCase(_repository.Object);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidInputException_WhenCountryCodeIsNullOrEmpty()
    {
        string? countryCode = null;

        Func<Task> action = async () => await _useCase.Execute(countryCode!);

        await action.Should().ThrowAsync<GetIdentifierRegistriesException.InvalidInputException>()
            .WithMessage("Country code cannot be null or empty.");
    }

    [Fact]
    public async Task ShouldReturnCorrectCountryIdentifiers_WhenRepositoryReturnsData()
    {

        var countryCode = "US";
        var expectedData = new List<IdentifierRegistries>
        {
            new IdentifierRegistries
            {
                Id = 1,
                CountryCode = "US",
                Scheme = "ISO",
                RegisterName = "ISO 3166",
                CreatedOn = DateTimeOffset.UtcNow.AddDays(-10),
                UpdatedOn = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new IdentifierRegistries
            {
                Id = 2,
                CountryCode = "US",
                Scheme = "GS1",
                RegisterName = "GS1 US",
                CreatedOn = DateTimeOffset.UtcNow.AddDays(-20),
                UpdatedOn = DateTimeOffset.UtcNow.AddDays(-2)
            }
        };

        _repository
            .Setup(repo => repo.GetIdentifierRegistriesAsync(countryCode))
            .ReturnsAsync(expectedData);

        var result = await _useCase.Execute(countryCode);

        result.Should().NotBeNull();
        result.Should().Equals(2);
        result.First().Scheme.Should().Be("ISO");
        result.Last().Scheme.Should().Be("GS1");
    }

    [Fact]
    public async Task Execute_ShouldThrowNotFoundException_WhenIdentifiersAreNullOrEmpty()
    {
        var countryCode = "US";

        _repository.Setup(repo => repo.GetIdentifierRegistriesAsync(countryCode))
                    .ReturnsAsync(new List<IdentifierRegistries>());


        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(countryCode));
    }   
}