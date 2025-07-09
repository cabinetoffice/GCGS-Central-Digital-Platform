namespace CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;

public class GetCpvChildrenUseCase
{
    private readonly ICpvCodeRepository _repository;

    public GetCpvChildrenUseCase(ICpvCodeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CpvCode>> ExecuteAsync(string code)
    {
        var parent = await _repository.FindByCodeAsync(code);

        if (parent == null)
        {
            return Enumerable.Empty<CpvCode>();
        }

        return await _repository.GetChildrenAsync(parent);
    }
}

public interface ICpvCodeRepository
{
    Task<CpvCode?> FindByCodeAsync(string code);
    Task<IEnumerable<CpvCode>> GetChildrenAsync(CpvCode parent);
}