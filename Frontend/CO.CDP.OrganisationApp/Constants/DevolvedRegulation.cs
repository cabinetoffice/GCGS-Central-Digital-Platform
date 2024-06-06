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
    //public static Organisation.WebApiClient.DevolvedRegulation AsApiDevolvedRegulations(this DevolvedRegulation devolvedRegulations)
    //{
    //    return devolvedRegulations switch
    //    {
    //        DevolvedRegulation.NorthernIreland => Organisation.WebApiClient.DevolvedRegulation.NorthernIreland,
    //        DevolvedRegulation.Scotland => Organisation.WebApiClient.DevolvedRegulation.Scotland,
    //        DevolvedRegulation.Wales => Organisation.WebApiClient.DevolvedRegulation.Wales,
    //        _ => throw new NotImplementedException(),
    //    };
    //}

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