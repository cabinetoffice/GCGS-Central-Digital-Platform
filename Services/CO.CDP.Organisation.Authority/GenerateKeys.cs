using System.Security.Cryptography;

namespace CO.CDP.Organisation.Authority;

public class RsaKeys
{
    public static void GenerateKeys(out string privateKey, out string publicKey)
    {
        using var rsa = new RSACryptoServiceProvider(2048);

        try
        {
            // Export the public key
            var publicKeyParameters = rsa.ExportParameters(false);
            publicKey = rsa.ExportRSAPublicKeyPem();

            // Export the private key
            var privateKeyParameters = rsa.ExportParameters(true);
            privateKey = rsa.ExportRSAPrivateKeyPem();
        }
        finally
        {
            // https://stackoverflow.com/a/5845191
            rsa.PersistKeyInCsp = false;
        }
    }
}