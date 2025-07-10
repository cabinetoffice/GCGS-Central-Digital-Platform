using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;
using Microsoft.EntityFrameworkCore;
using UseCaseCpvCode = CO.CDP.RegisterOfCommercialTools.WebApi.UseCases.CpvCode;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Persistence;

public class DatabaseCpvCodeRepository(OrganisationInformationContext context) : ICpvCodeRepository
{
    public async Task<IEnumerable<UseCaseCpvCode>> GetAllAsync()
    {
        var allCodes = await context.CpvCodes.ToListAsync();
        return allCodes.Select(c => new UseCaseCpvCode(
            c.Code,
            c.Description)
        );
    }
}
