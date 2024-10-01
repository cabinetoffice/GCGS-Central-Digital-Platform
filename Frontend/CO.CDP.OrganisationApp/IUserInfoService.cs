public interface IUserInfoService
{
    public Guid? GetOrganisationId();
    public Task<ICollection<String>> GetOrganisationUserScopes();

    public Task<bool> UserHasScope(string scope);
}