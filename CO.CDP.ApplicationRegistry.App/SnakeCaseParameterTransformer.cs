using System.Text.RegularExpressions;

namespace CO.CDP.ApplicationRegistry.App;

public partial class SnakeCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is null)
        {
            return null;
        }

        var str = value.ToString();
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }

        return SnakeCaseRegex().Replace(str, "$1_$2").ToLowerInvariant();
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex SnakeCaseRegex();
}
