namespace CO.CDP.OrganisationInformation.Persistence;

public interface IOrganisationJoinRequestRepository : IDisposable
{
    Task<OrganisationJoinRequest?> Find(Guid organisationJoinRequestId);

    void Save(OrganisationJoinRequest organisationJoinRequest);

    Task<IEnumerable<OrganisationJoinRequest>> FindByOrganisation(Guid organisationJoinRequestId);
}