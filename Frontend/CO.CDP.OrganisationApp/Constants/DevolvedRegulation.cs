using WebApiClient = CO.CDP.Organisation.WebApiClient;
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

    public static WebApiClient.DevolvedRegulation AsApiClientDevolvedRegulation(this DevolvedRegulation devolvedRegulation)
    {
        switch (devolvedRegulation)
        {
            case DevolvedRegulation.NorthernIreland: return WebApiClient.DevolvedRegulation.NorthernIreland;
            case DevolvedRegulation.Scotland: return WebApiClient.DevolvedRegulation.Scotland;
            case DevolvedRegulation.Wales: return WebApiClient.DevolvedRegulation.Wales;
            default: return WebApiClient.DevolvedRegulation.NorthernIreland;
        }
    }

    public static ICollection<WebApiClient.DevolvedRegulation> AsApiClientDevolvedRegulationList(this List<DevolvedRegulation> devolvedRegulation)
    {
        var list = new List<WebApiClient.DevolvedRegulation>();

        foreach (var item in devolvedRegulation)
        {
            list.Add(item.AsApiClientDevolvedRegulation());
        }

        return list;
    }
}