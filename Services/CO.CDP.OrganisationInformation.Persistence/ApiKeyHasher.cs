using System.Security.Cryptography;
using System.Text;

namespace CO.CDP.OrganisationInformation.Persistence;

/// <summary>
/// Hashes API keys for storage and lookup. API keys are high-entropy random values,
/// so an unsalted SHA-256 (lowercase hex) is sufficient and keeps lookups deterministic.
/// Must stay in sync with the SQL used in migrations:
/// <c>encode(sha256(convert_to(key, 'UTF8')), 'hex')</c>.
/// </summary>
public static class ApiKeyHasher
{
    private const int HintLength = 4;

    public static string Hash(string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey))).ToLowerInvariant();
    }

    public static string Hint(string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        return apiKey.Length <= HintLength ? apiKey : apiKey[^HintLength..];
    }
}
