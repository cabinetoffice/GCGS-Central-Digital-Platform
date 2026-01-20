namespace CO.CDP.ApplicationRegistry.App;

public interface IAppSession
{
    T? Get<T>(string key);
    void Set<T>(string key, T value);
    void Remove(string key);
    void Clear();
}

