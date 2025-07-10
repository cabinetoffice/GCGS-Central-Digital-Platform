namespace CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;

public class SearchCpvCodesUseCase
{
    public Task<IEnumerable<CpvCode>> ExecuteAsync(string search)
    {
        return Task.FromResult(Enumerable.Empty<CpvCode>());
    }
}

public record CpvCode(string Code, string Name);

