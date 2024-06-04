namespace CO.CDP.OrganisationApp;

public interface ISession
{
    T? Get<T>(string key);

    void Set<T>(string key, T value);

    void Remove(string key);

    void Clear();
}