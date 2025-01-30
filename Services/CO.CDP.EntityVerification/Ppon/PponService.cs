using CO.CDP.OrganisationInformation;
using System.Text;

namespace CO.CDP.EntityVerification.Ppon;

public class PponService : IPponService
{
    private readonly char[] ValidAlphaChars = "BCDGHJLMNPQRTVWXYZ".ToCharArray();
    private readonly char[] ValidLastAlphaCharsForNonConsortium = "DGHJLMNPQRTVWXYZ".ToCharArray();
    private readonly char[] ValidNumericChars = "123456789".ToCharArray();
    private readonly Random RandomGenerator = new();

    public string GeneratePponId(OrganisationType type)
    {
        return $"P{GenerateCodePattern(type).Substring(1)}";
    }

    private string GenerateCodePattern(OrganisationType type)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < 12; i++)
        {
            if (i == 11)
            {
                char nextChar = ValidLastAlphaCharsForNonConsortium[RandomGenerator.Next(ValidLastAlphaCharsForNonConsortium.Length)];

                if (type == OrganisationType.InformalConsortium)
                {
                    nextChar = 'C';
                }

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