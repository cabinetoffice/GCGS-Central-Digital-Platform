using System.Text.Json;

namespace CO.CDP.OrganisationInformation.Persistence;

public static class NpgsqlDataReaderExtension
{
    public static string? GetNullableString(this Npgsql.NpgsqlDataReader reader, string fieldname)
    {
        int colIndex = reader.GetOrdinal(fieldname);
        if (!reader.IsDBNull(colIndex))
            return reader.GetString(colIndex);
        return null;
    }

    public static T? GetJsonObject<T>(this Npgsql.NpgsqlDataReader reader, string fieldname)
    {
        var jsonData = reader[fieldname];
        if (jsonData != DBNull.Value)
        {
            var jsonString = jsonData as string;
            return JsonSerializer.Deserialize<T>(jsonString!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        return default;
    }
}