using System.Globalization;
using System.Text.RegularExpressions;

namespace CO.CDP.Mvc.Validation;

// Sourced from GDS repository
// https://github.com/alphagov/notifications-utils/blob/main/tests/recipient_validation/test_email_address.py
public static class EmailAddressValidator
{
    private static readonly string ValidLocalChars = @"a-zA-Z0-9.!#$%&'*+/=?^_`{|}~\-";
    private static readonly string EmailRegexPattern = $@"^[{ValidLocalChars}]+@([^.@][^@\s]+)$";
    private static readonly string HostnamePartRegex = new(@"^(xn|[a-z0-9]+)(-?-[a-z0-9]+)*$");
    private static readonly string TldPartRegex = new(@"^([a-z]{2,63}|xn--([a-z0-9]+-)*[a-z0-9]+)$");

    public static bool IsValid(string emailAddress)
    {
        if (!ValidateFormat(emailAddress))
        {
            return false;
        }

        if (emailAddress.Length > 320)
        {
            return false;
        }

        if (emailAddress.Contains(".."))
        {
            return false;
        }

        string hostname = GetHostname(emailAddress);
        if (!ValidateHostname(hostname))
        {
            return false;
        }

        return true;
    }

    private static bool ValidateFormat(string emailAddress)
    {
        return Regex.IsMatch(emailAddress, EmailRegexPattern);
    }

    private static string GetHostname(string emailAddress)
    {
        var match = Regex.Match(emailAddress, EmailRegexPattern);

        return match.Groups[1].Value;
    }

    private static bool ValidateHostname(string hostname)
    {
        try
        {
            hostname = new IdnMapping().GetAscii(hostname);
        }
        catch (ArgumentException)
        {
            return false;
        }

        var parts = hostname.Split('.');
        if (hostname.Length > 253 || parts.Length < 2)
        {
            return false;
        }

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part) || part.Length > 63 || !Regex.IsMatch(part, HostnamePartRegex, RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        if (!Regex.IsMatch(parts[parts.Length - 1], TldPartRegex, RegexOptions.IgnoreCase))
        {
            return false;
        }

        return true;
    }
}