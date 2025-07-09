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
    public async Task ExecuteAsync_WhenCodeIsValid_ReturnsDirectChildrenDivisions()
    {
        var parentCode = "03000000-1";
        var parentCpvCode = new CpvCode(parentCode, "Agricultural, farming, fishing, forestry and related products");
        var childrenCpvCodes = new List<CpvCode>
        {
            new("03100000-2", "Agricultural and horticultural products"),
            new("03200000-3", "Cereals, potatoes, vegetables, fruits and nuts"),
            new("03300000-4", "Farming, hunting and fishing products"),
            new("03400000-5", "Forestry and logging products"),
        };

        _repositoryMock.Setup(r => r.FindByCodeAsync(parentCode)).ReturnsAsync(parentCpvCode);
        _repositoryMock.Setup(r => r.GetChildrenAsync(parentCpvCode)).ReturnsAsync(childrenCpvCodes);

        var result = await _useCase.ExecuteAsync(parentCode);

        var expectedCodes = new[] { "03100000-2", "03200000-3", "03300000-4", "03400000-5" };
        Assert.Equal(expectedCodes, result.Select(c => c.Code).ToList());
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueryingGroup_ReturnsDirectChildrenClasses()
    {
        var parentCode = "03100000-2";
        var parentCpvCode = new CpvCode(parentCode, "Agricultural and horticultural products");
        var childrenCpvCodes = new List<CpvCode>
        {
            new("03110000-5", "Crops, products of market gardening and horticulture"),
            new("03120000-8", "Horticultural and nursery products"),
            new("03130000-1", "Beverage and spice crops"),
            new("03140000-4", "Animal products and related products"),
        };

        _repositoryMock.Setup(r => r.FindByCodeAsync(parentCode)).ReturnsAsync(parentCpvCode);
        _repositoryMock.Setup(r => r.GetChildrenAsync(parentCpvCode)).ReturnsAsync(childrenCpvCodes);

        var result = await _useCase.ExecuteAsync(parentCode);

        var expectedCodes = new[] { "03110000-5", "03120000-8", "03130000-1", "03140000-4" };
        Assert.Equal(expectedCodes, result.Select(c => c.Code).ToList());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCodeIsNotFound_ReturnsEmptyList()
    {
        var nonExistentCode = "99999999-9";
        _repositoryMock.Setup(r => r.FindByCodeAsync(nonExistentCode)).ReturnsAsync((CpvCode?)null);

        var result = await _useCase.ExecuteAsync(nonExistentCode);

        Assert.Empty(result);
    }
}
