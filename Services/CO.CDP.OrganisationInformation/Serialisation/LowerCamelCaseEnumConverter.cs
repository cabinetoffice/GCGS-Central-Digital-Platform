using System.Text.Json;
using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation.Serialisation;

public class LowerCamelCaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();

        if (Enum.TryParse(enumString, true, out T value))
        {
            return value;
        }

        throw new JsonException($"Unable to convert \"{enumString}\" to enum \"{typeof(T)}\"");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var enumString = value.ToString();

        writer.WriteStringValue(char.ToLowerInvariant(enumString[0]) + enumString[1..]);
    }
}