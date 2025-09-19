namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public enum Culture
{
    English,
    Welsh
}

public static class CultureExtensions
{
    public static string ToCode(this Culture culture)
    {
        return culture switch
        {
            Culture.English => "en",
            Culture.Welsh => "cy",
            _ => "en"
        };
    }

    public static Culture FromCode(string code)
    {
        return code?.ToLowerInvariant() switch
        {
            "cy" => Culture.Welsh,
            _ => Culture.English
        };
    }

    public static bool IsWelsh(this Culture culture) => culture == Culture.Welsh;
    public static bool IsEnglish(this Culture culture) => culture == Culture.English;
}