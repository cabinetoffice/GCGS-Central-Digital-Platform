using System.Security.Cryptography;

namespace CO.CDP.Authentication;

public static class SecretHasher
{
    public static string Hash(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        var salt = Convert.FromHexString("1DC8744FE373E1F2A8F68A34FC3372F8"); //RandomNumberGenerator.GetBytes(16); // 128 bits

        var hash = Rfc2898DeriveBytes.Pbkdf2(secret, salt, 50000, HashAlgorithmName.SHA256, 32);

        return Convert.ToHexString(hash);
    }
}