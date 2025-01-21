using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationsTotalUseCase(IOrganisationRepository organisationRepository)
    : IUseCase<OrganisationTypeQuery, int>
{
    public async Task<int> Execute(OrganisationTypeQuery command)
    {
        return await organisationRepository.GetTotalCount(command.Type);
    }
}