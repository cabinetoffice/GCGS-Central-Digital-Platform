using NanoidDotNet;

namespace CO.CDP.DataSharing.WebApi.Extensions;

public static class ShareCodeExtensions
{
    public static string GenerateShareCodeString()
    {
        return Nanoid.Generate(Nanoid.Alphabets.NoLookAlikesSafe, 8);
    }
}
