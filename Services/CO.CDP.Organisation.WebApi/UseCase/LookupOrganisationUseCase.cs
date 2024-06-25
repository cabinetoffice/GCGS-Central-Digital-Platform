using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class LookupOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<string, Model.Organisation?>
{
    public async Task<Model.Organisation?> Execute(string query)
    {
        if (IsIdentifier(query, out var scheme, out var id))
        {
            return await organisationRepository.FindByIdentifier(scheme, id)
                .AndThen(mapper.Map<Model.Organisation>);
        }
        else
        {
            return await organisationRepository.FindByName(query)
                .AndThen(mapper.Map<Model.Organisation>);
        }

    }
    private bool IsIdentifier(string query, out string scheme, out string id)
    {
        var parts = query.Split(':');
        if (parts.Length == 2)
        {
            scheme = parts[0];
            id = parts[1];
            return true;
        }
        scheme = string.Empty;
        id = string.Empty;
        return false;
    }
}