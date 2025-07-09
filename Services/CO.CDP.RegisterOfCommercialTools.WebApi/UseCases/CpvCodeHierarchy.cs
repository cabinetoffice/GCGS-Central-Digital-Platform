namespace CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;

public static class CpvCodeHierarchy
{
    public static bool IsChildOf(CpvCode parent, CpvCode child)
    {
        var parentCodePrefix = parent.Code.Split('-')[0];
        var childCodePrefix = child.Code.Split('-')[0];

        var significantDigits = GetSignificantDigits(parentCodePrefix);
        if (parent.Code == child.Code || !childCodePrefix.StartsWith(parentCodePrefix.Substring(0, significantDigits)))
        {
            return false;
        }

        var childCodePadded = childCodePrefix.PadRight(8, '0');

        return (significantDigits == 2 && childCodePadded[significantDigits] != '0' &&
                childCodePadded.Substring(significantDigits + 1) == "00000") ||
               (significantDigits == 3 && childCodePadded[significantDigits] != '0' &&
                childCodePadded.Substring(significantDigits + 1) == "0000") ||
               (significantDigits == 4 && childCodePadded[significantDigits] != '0' &&
                childCodePadded.Substring(significantDigits + 1) == "000") ||
               (significantDigits == 5 && childCodePadded[significantDigits] != '0' &&
                childCodePadded.Substring(significantDigits + 1) == "00");
    }

    public static int GetSignificantDigits(string codePrefix)
    {
        if (codePrefix.EndsWith("000000")) return 2;
        if (codePrefix.EndsWith("00000")) return 3;
        if (codePrefix.EndsWith("0000")) return 4;
        if (codePrefix.EndsWith("000")) return 5;
        return codePrefix.TrimEnd('0').Length;
    }
}