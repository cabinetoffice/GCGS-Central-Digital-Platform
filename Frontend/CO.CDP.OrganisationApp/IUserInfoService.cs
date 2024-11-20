namespace CO.CDP.OrganisationApp;

public interface IUserInfoService
{
    public Guid? GetOrganisationId();
    public Task<bool> IsViewer();
    Task<bool> HasTenant();
}