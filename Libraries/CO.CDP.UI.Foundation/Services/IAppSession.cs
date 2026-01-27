using System;

namespace CO.CDP.UI.Foundation.Services;

public interface IAppSession
{
    T? Get<T>(string key);
    void Set<T>(string key, T value);
    void Remove(string key);
    void Clear();
}
