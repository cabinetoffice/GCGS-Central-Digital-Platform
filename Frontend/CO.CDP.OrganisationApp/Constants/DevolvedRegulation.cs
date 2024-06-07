using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Constants;

public enum DevolvedRegulation
{
    NorthernIreland = 1,
    Scotland,
    Wales
}

public static class DevolvedRegulationsExtensions
{
    public static string Description(this DevolvedRegulation devolvedRegulations)
    {        
        return devolvedRegulations switch
        {
            DevolvedRegulation.NorthernIreland => "Northern Ireland",
            DevolvedRegulation.Scotland => "Scotland",
            DevolvedRegulation.Wales => "Wales",
            _ => throw new NotImplementedException()
        };
    }
}