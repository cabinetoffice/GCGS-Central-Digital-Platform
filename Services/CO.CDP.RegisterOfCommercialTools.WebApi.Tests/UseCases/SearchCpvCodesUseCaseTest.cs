using CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.UseCases;

public class SearchCpvCodesUseCaseTest
{
    [Fact]
    public async Task ExecuteAsync_WhenNoCodesMatch_ReturnsEmptyList()
    {
        var useCase = new SearchCpvCodesUseCase();
        var result = await useCase.ExecuteAsync("non-existent");
        Assert.Empty(result);
    }
}

