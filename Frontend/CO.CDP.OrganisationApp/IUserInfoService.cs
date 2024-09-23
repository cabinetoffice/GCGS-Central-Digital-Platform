using CO.CDP.OrganisationApp.Models;

public interface IUserInfoService
{
    public Task<ICollection<String>> GetOrganisationUserScopes();

    public Task<bool> UserHasScope(string scope);
}