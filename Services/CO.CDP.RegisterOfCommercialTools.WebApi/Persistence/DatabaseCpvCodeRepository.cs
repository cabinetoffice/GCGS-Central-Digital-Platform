using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;
using Microsoft.EntityFrameworkCore;
using UseCaseCpvCode = CO.CDP.RegisterOfCommercialTools.WebApi.UseCases.CpvCode;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Persistence;

public class DatabaseCpvCodeRepository(OrganisationInformationContext context) : ICpvCodeRepository
{
    public Task<UseCaseCpvCode?> FindByCodeAsync(string code)
    {
        return context.CpvCodes
            .Where(c => c.Code == code)
            .Select(c => new UseCaseCpvCode(c.Code, c.Description))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UseCaseCpvCode>> GetChildrenAsync(UseCaseCpvCode parent)
    {
        var parentCodePrefix = parent.Code.Split('-')[0];

        var significantDigits = CpvCodeHierarchy.GetSignificantDigits(parentCodePrefix);
        var parentCodePrefixSignificant = parentCodePrefix.Substring(0, significantDigits);

        var query = context.CpvCodes.AsQueryable();

        query = query.Where(c => c.Code != parent.Code && c.Code.StartsWith(parentCodePrefixSignificant));

        var potentialChildren = await query.ToListAsync();

        return potentialChildren.Where(c => CpvCodeHierarchy.IsChildOf(parent, new UseCaseCpvCode(c.Code, c.Description)))
            .Select(c => new UseCaseCpvCode(c.Code, c.Description));
    }
}
