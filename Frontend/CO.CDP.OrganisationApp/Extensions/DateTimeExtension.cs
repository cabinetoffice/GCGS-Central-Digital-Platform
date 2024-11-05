namespace CO.CDP.OrganisationApp.Extensions;

public static class DateTimeExtension
{
    public static string ToFormattedString(this DateTimeOffset dateTime)
    {
        return dateTime.ToString("dd MMMM yyyy");
    }
}