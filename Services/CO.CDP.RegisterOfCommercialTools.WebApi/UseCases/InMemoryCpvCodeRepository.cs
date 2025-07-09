namespace CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;

public class InMemoryCpvCodeRepository : ICpvCodeRepository
{
    private readonly List<CpvCode> _codes = new()
    {
        new("03000000-1", "Agricultural, farming, fishing, forestry and related products"),
        new("03100000-2", "Agricultural and horticultural products"),
        new("03110000-5", "Crops, products of market gardening and horticulture"),
        new("03200000-3", "Cereals, potatoes, vegetables, fruits and nuts"),
        new("03300000-4", "Farming, hunting and fishing products"),
        new("03400000-5", "Forestry and logging products"),
        new("15000000-8", "Food, beverages, tobacco and related products")
    };

    public Task<IEnumerable<CpvCode>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<CpvCode>>(_codes);
    }
}