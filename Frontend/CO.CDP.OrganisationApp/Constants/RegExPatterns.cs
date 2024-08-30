namespace CO.CDP.OrganisationApp.Constants;

public static class RegExPatterns
{
    public const string EmailAddress = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
    public const string Day = @"^(0?[1-9]|[12][0-9]|3[01])$";
    public const string Month = @"^(0?[1-9]|1[0-2])$";
    public const string Year = @"^\d{4}$";
    public const string OrganisationId = @"/organisation/([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})";
}