using System.Text;

namespace CO.CDP.EntityVerification.Ppon;

public class PponService : IPponService
{
    private readonly char[] ValidAlphaChars = "BCDGHJLMNPQRTVWXYZ".ToCharArray();
    private readonly char[] ValidLastAlphaChars = "DGHJLMNPQRTVWXYZ".ToCharArray();
    private readonly char[] ValidNumericChars = "123456789".ToCharArray();
    private readonly Random RandomGenerator = new();

    public string GeneratePponId()
    {
        // TODO: requires new param to indicate type of org e.g. Consortium will have a C as the last character in future.

        return $"P{GenerateCodePattern().Substring(1)}";
    }

    private string GenerateCodePattern()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < 12; i++)
        {
            if (i == 11)
            {
                char nextChar = ValidLastAlphaChars[RandomGenerator.Next(ValidLastAlphaChars.Length)];

                builder.Append(nextChar);
            }
            else
            {
                char nextChar = (i / 4) % 2 == 0
                        ? ValidAlphaChars[RandomGenerator.Next(ValidAlphaChars.Length)]
                        : ValidNumericChars[RandomGenerator.Next(ValidNumericChars.Length)];

                builder.Append(nextChar);
            }

            if ((i + 1) % 4 == 0 && i != 11) // Add hyphens after every 4 characters, except the last group
            {
                builder.Append('-');
            }
        }
        return builder.ToString();
    }
}