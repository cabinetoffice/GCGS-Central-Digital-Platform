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
        var allCodes = await _repository.GetAllAsync();
        var cpvCodes = allCodes.ToList();
        var parent = cpvCodes.FirstOrDefault(c => c.Code.StartsWith(code));

        if (parent == null)
        {
            return Enumerable.Empty<CpvCode>();
        }

        var parentCodePrefix = parent.Code.Split('-')[0];

        var significantDigits = 0;
        if (parentCodePrefix.EndsWith("000000")) significantDigits = 2;
        else if (parentCodePrefix.EndsWith("00000")) significantDigits = 3;
        else if (parentCodePrefix.EndsWith("0000")) significantDigits = 4;
        else if (parentCodePrefix.EndsWith("000")) significantDigits = 5;
        else significantDigits = parentCodePrefix.TrimEnd('0').Length;

        return cpvCodes.Where(c =>
        {
            if (c.Code == parent.Code) return false;

            var childCodePrefix = c.Code.Split('-')[0];
            if (!childCodePrefix.StartsWith(parentCodePrefix.Substring(0, significantDigits))) return false;

            var childCodePadded = childCodePrefix.PadRight(8, '0');

            bool isChild =
                significantDigits == 2 && childCodePadded[significantDigits] != '0' &&
                childCodePadded.Substring(significantDigits + 1) == "00000" ||
                significantDigits == 3 && childCodePadded[significantDigits] != '0' &&
                childCodePadded.Substring(significantDigits + 1) == "0000" ||
                significantDigits == 4 && childCodePadded[significantDigits] != '0' &&
                childCodePadded.Substring(significantDigits + 1) == "000" || significantDigits == 5 &&
                childCodePadded[significantDigits] != '0' && childCodePadded.Substring(significantDigits + 1) == "00";

            return isChild;
        }).ToList();
    }
}

public interface ICpvCodeRepository
{
    Task<IEnumerable<CpvCode>> GetAllAsync();
}