using System.Text;

namespace CO.CDP.EntityVerification.Ppon;

public class PponService : IPponService
{
    private readonly char[] CharPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    private readonly char[] EuansIdea = "ABCDEFGHIJKLMNPQRSTUVWXYZ".ToCharArray();
    private readonly char[] EuansIdea2 = "123456789".ToCharArray();
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
                        ? EuansIdea[RandomGenerator.Next(EuansIdea.Length)]
                        : EuansIdea2[RandomGenerator.Next(EuansIdea2.Length)];



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
        var builder = new StringBuilder();
        for (int i = 0; i < 2; i++)
        {
            builder.Append(CharPool[RandomGenerator.Next(CharPool.Length)]);
        }
        return builder.ToString();
    }
}