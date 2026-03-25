using System.Text.Json;

namespace CO.CDP.UserManagement.Core;

public static class JsonHelper
{
    public static T? TryDeserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
