namespace CO.CDP.OrganisationInformation.Persistence;

public interface IAuthorityRepository
{
    Task<RefreshToken?> Find(string tokenHash);

    Task Save(RefreshToken refreshToken);
}