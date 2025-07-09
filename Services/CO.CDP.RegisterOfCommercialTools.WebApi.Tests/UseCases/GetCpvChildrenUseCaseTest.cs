using CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.UseCases;

public class GetCpvChildrenUseCaseTest
{
    private readonly Mock<ICpvCodeRepository> _repositoryMock;
    private readonly GetCpvChildrenUseCase _useCase;

    public GetCpvChildrenUseCaseTest()
    {
        _repositoryMock = new Mock<ICpvCodeRepository>();
        _useCase = new GetCpvChildrenUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueryingDivision_ReturnsDirectChildrenGroups()
    {
        var cpvCodes = new List<CpvCode>
        {
            new("03000000-1", "Agricultural, farming, fishing, forestry and related products"),
            new("03100000-2", "Agricultural and horticultural products"),
            new("03110000-5", "Crops, products of market gardening and horticulture"),
            new("03200000-3", "Cereals, potatoes, vegetables, fruits and nuts"),
            new("03300000-4", "Farming, hunting and fishing products"),
            new("03400000-5", "Forestry and logging products"),
            new("15000000-8", "Food, beverages, tobacco and related products")
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cpvCodes);

        var result = await _useCase.ExecuteAsync("03000000");

        var expectedCodes = new[] { "03100000-2", "03200000-3", "03300000-4", "03400000-5" };
        Assert.Equal(expectedCodes, result.Select(c => c.Code).ToList());
    }
}
