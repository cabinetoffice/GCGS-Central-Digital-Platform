
namespace CO.CDP.OrganisationApp;

public interface ITempDataService
{
    void Put<T>(string key, T value);

    T? Get<T>(string key);

    T GetOrDefault<T>(string key) where T : new();

    T? Peek<T>(string key);

    T PeekOrDefault<T>(string key) where T : new();

    void Remove(string key);
}