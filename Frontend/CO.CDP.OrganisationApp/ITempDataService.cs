namespace CO.CDP.OrganisationApp;

public interface ITempDataService
{
    void Put<T>(string key, T value) where T : new();

    T GetOrDefault<T>(string key) where T : new();

    T PeekOrDefault<T>(string key) where T : new();

    void Remove(string key);
}