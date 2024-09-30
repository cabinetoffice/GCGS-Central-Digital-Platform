using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CO.CDP.OrganisationApp;

public class TempDataService(ITempDataDictionary tempData) : ITempDataService
{
    public IEnumerable<string> Keys => tempData.Keys;

    public void Put<T>(string key, T value)
    {
        tempData[key] = JsonSerializer.Serialize(value);
    }

    public T? Get<T>(string key)
    {
        var o = tempData[key];
        return o == null ? default : JsonSerializer.Deserialize<T>((string)o);
    }

    public T GetOrDefault<T>(string key) where T : new()
    {
        return Get<T>(key) ?? new T();
    }

    public T? Peek<T>(string key)
    {
        object? o = tempData.Peek(key);
        return o == null ? default : JsonSerializer.Deserialize<T>((string)o);
    }

    public T PeekOrDefault<T>(string key) where T : new()
    {
        return Peek<T>(key) ?? new T();
    }

    public void Remove(string key)
    {
        tempData.Remove(key);
    }
}