using System.Text;

namespace CO.CDP.EntityVerification.Ppon;

public class PponService : IPponService
{
    private readonly char[] ValidAlphaChars = "ABCDEFGHIJKLMNPQRSTUVWXYZ".ToCharArray();
    private readonly char[] ValidNumericChars = "123456789".ToCharArray();
    private readonly Random RandomGenerator = new();

    public string GeneratePponId()
    {
        return $"{GenerateCodePattern()}-{GenerateMultiplier()}";
    }

    private string GenerateCodePattern()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < 12; i++)
        {
            char nextChar = (i / 4) % 2 == 0
                        ? ValidAlphaChars[RandomGenerator.Next(ValidAlphaChars.Length)]
                        : ValidNumericChars[RandomGenerator.Next(ValidNumericChars.Length)];



            builder.Append(nextChar);

            if ((i + 1) % 4 == 0 && i != 11) // Add hyphens after every 4 characters, except the last group
            {
                builder.Append('-');
            }
        }
        return builder.ToString();
    }

    private string GenerateMultiplier()
    {
        char[] ValidCharPool = ValidAlphaChars.Concat(ValidNumericChars).ToArray();
        var builder = new StringBuilder();

        for (int i = 0; i < 2; i++)
        {
            builder.Append(ValidCharPool[RandomGenerator.Next(ValidCharPool.Length)]);
        }
        return builder.ToString();
    }
}