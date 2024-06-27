using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class LookupOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<OrganisationQuery, Model.Organisation?>
{
    public async Task<Model.Organisation?> Execute(OrganisationQuery query)
    {
        if (!query.IsValid)
        {
            throw new ArgumentException("Organisation name or identifier must be provided.");
        }

        if (!string.IsNullOrEmpty(query.Identifier))
        {
            if (IsIdentifier(query.Identifier, out var scheme, out var id))
            {
                return await organisationRepository.FindByIdentifier(scheme, id)
                    .AndThen(mapper.Map<Model.Organisation>);
            }
            throw new ArgumentException("Invalid identifier format.");
        }

        if (!string.IsNullOrEmpty(query.Name))
        {
            return await organisationRepository.FindByName(query.Name)
                .AndThen(mapper.Map<Model.Organisation>);
        }

        return null;
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

public class OrganisationQuery
{
    public string? Name { get; }
    public string? Identifier { get; }

    public OrganisationQuery(string? name = null, string? identifier = null)
    {
        Name = name;
        Identifier = identifier;
    }

    public bool IsValid => !string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Identifier);
}