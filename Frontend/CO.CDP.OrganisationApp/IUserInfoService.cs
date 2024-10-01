public interface IUserInfoService
{
    public Task<ICollection<String>> GetUserScopes();
    public Guid? GetOrganisationId();
    public Task<ICollection<String>> GetOrganisationUserScopes();
    public Task<bool> UserHasScope(string scope);
}