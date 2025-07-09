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

    [Fact]
    public async Task ExecuteAsync_WhenQueryingGroup_ReturnsDirectChildrenClasses()
    {
        var cpvCodes = new List<CpvCode>
        {
            new("03000000-1", "Agricultural, farming, fishing, forestry and related products"),
            new("03100000-2", "Agricultural and horticultural products"),
            new("03110000-5", "Crops, products of market gardening and horticulture"),
            new("03120000-8", "Horticultural and nursery products"),
            new("03130000-1", "Beverage and spice crops"),
            new("03140000-4", "Animal products and related products"),
            new("15000000-8", "Food, beverages, tobacco and related products")
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cpvCodes);

        var result = await _useCase.ExecuteAsync("03100000");

        var expectedCodes = new[] { "03110000-5", "03120000-8", "03130000-1", "03140000-4" };
        Assert.Equal(expectedCodes, result.Select(c => c.Code).ToList());
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueryingClass_ReturnsDirectChildrenCategories()
    {
        var cpvCodes = new List<CpvCode>
        {
            new("03110000-5", "Crops, products of market gardening and horticulture"),
            new("03111000-8", "Seeds"),
            new("03112000-1", "Unmanufactured tobacco"),
            new("03113000-4", "Plants used for sugar manufacturing"),
            new("03114000-7", "Straw and forage"),
            new("03115000-0", "Raw vegetable materials"),
            new("03116000-3", "Natural rubber and latex, and associated products"),
            new("03117000-6", "Plants used in specific fields"),
            new("15000000-8", "Food, beverages, tobacco and related products")
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cpvCodes);

        var result = await _useCase.ExecuteAsync("03110000");

        var expectedCodes = new[]
        {
            "03111000-8", "03112000-1", "03113000-4", "03114000-7", "03115000-0", "03116000-3", "03117000-6"
        };
        Assert.Equal(expectedCodes, result.Select(c => c.Code).ToList());
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueryingCategory_ReturnsDirectChildrenSubCategories()
    {
        var cpvCodes = new List<CpvCode>
        {
            new("03113000-4", "Plants used for sugar manufacturing"),
            new("03113100-7", "Sugar beet"),
            new("03113200-0", "Sugar cane"),
            new("15000000-8", "Food, beverages, tobacco and related products")
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cpvCodes);

        var result = await _useCase.ExecuteAsync("03113000");

        var expectedCodes = new[] { "03113100-7", "03113200-0" };
        Assert.Equal(expectedCodes, result.Select(c => c.Code).ToList());
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueryingSubCategory_ReturnsNoChildren()
    {
        var cpvCodes = new List<CpvCode>
        {
            new("03113100-7", "Sugar beet"),
            new("15000000-8", "Food, beverages, tobacco and related products")
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cpvCodes);

        var result = await _useCase.ExecuteAsync("03113100");

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCodeDoesNotExist_ReturnsEmptyList()
    {
        var cpvCodes = new List<CpvCode>
        {
            new("03113100-7", "Sugar beet"),
            new("15000000-8", "Food, beverages, tobacco and related products")
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cpvCodes);

        var result = await _useCase.ExecuteAsync("99999999");

        Assert.Empty(result);
    }
}
