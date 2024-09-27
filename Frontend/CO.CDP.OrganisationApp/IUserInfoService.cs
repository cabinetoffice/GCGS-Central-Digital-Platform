public interface IUserInfoService
{
    public Task<ICollection<String>> GetUserScopes();
    public Task<ICollection<String>> GetOrganisationUserScopes();

    public Task<bool> UserHasScope(string scope);
}