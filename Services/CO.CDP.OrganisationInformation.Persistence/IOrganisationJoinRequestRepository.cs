namespace CO.CDP.OrganisationInformation.Persistence;

public interface IOrganisationJoinRequestRepository : IDisposable
{
    Task<OrganisationJoinRequest?> Find(Guid organisationJoinRequestId);
    Task<OrganisationJoinRequest?> Find(Guid organisationJoinRequestId, Guid organisationId);
    void Save(OrganisationJoinRequest organisationJoinRequest);
    Task<IEnumerable<OrganisationJoinRequest>> FindByOrganisation(Guid organisationJoinRequestId);
}