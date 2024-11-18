using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Tests.Persistence;
using CO.CDP.EntityVerification.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Moq;
using static CO.CDP.EntityVerification.UseCase.GetCountryIdentifiersUseCase;
using static CO.CDP.EntityVerification.UseCase.GetCountryIdentifiersUseCase.GetCountryIdentifiersException;

namespace CO.CDP.EntityVerification.Tests.UseCase;

using Moq;
using Xunit;

public class GetCountryIdentifiersUseCaseTest
{
    private readonly Mock<IPponRepository> _repository;
    private readonly GetCountryIdentifiersUseCase _useCase;

    public GetCountryIdentifiersUseCaseTest()
    {
        _repository = new Mock<IPponRepository>();

        _useCase = new GetCountryIdentifiersUseCase(_repository.Object);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidInputException_WhenCountryCodeIsNullOrEmpty()
    {
        string? countryCode = null;

        Func<Task> action = async () => await _useCase.Execute(countryCode!);

        await action.Should().ThrowAsync<GetCountryIdentifiersException.InvalidInputException>()
            .WithMessage("Country code cannot be null or empty.");
    }

    [Fact]
    public async Task ShouldReturnCorrectCountryIdentifiers_WhenRepositoryReturnsData()
    {

        var countryCode = "US";
        var expectedData = new List<CountryIndentifiers>
        {
            new CountryIndentifiers
            {
                Id = 1,
                CountryCode = "US",
                Scheme = "ISO",
                RegisterName = "ISO 3166",
                CreatedOn = DateTimeOffset.UtcNow.AddDays(-10),
                UpdatedOn = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new CountryIndentifiers
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
            .Setup(repo => repo.GetCountryIdentifiersAsync(countryCode))
            .ReturnsAsync(expectedData);

        var result = await _useCase.Execute(countryCode);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("ISO", result.First().Scheme);
        Assert.Equal("GS1", result.Last().Scheme);
    }

    [Fact]
    public async Task Execute_ShouldThrowNotFoundException_WhenIdentifiersAreNullOrEmpty()
    {
        var countryCode = "US";

        _repository.Setup(repo => repo.GetCountryIdentifiersAsync(countryCode))
                    .ReturnsAsync(new List<CountryIndentifiers>());


        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(countryCode));
    }   
}