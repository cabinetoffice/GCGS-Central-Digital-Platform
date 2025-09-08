using System.Globalization;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

public static class FeeConverter
{
    /// <summary>
    /// Converts proportion to percentage (e.g., 0.025 → 2.5)
    /// </summary>
    public static decimal ProportionToPercentage(decimal proportion)
    {
        return proportion * 100;
    }

    /// <summary>
    /// Converts percentage to proportion (e.g., 2.5 → 0.025)
    /// </summary>
    public static decimal PercentageToProportion(decimal percentage)
    {
        return percentage / 100;
    }

    /// <summary>
    /// Gets the highest participation fee percentage from a tender's participation fees
    /// </summary>
    public static decimal? GetMaxFeePercentage(List<ParticipationFee>? participationFees)
    {
        return participationFees?
            .Where(fee => fee.RelativeValue?.Proportion.HasValue == true)
            .Max(fee => ProportionToPercentage(fee.RelativeValue!.Proportion!.Value));
    }

    /// <summary>
    /// Creates an OData filter for participation fee proportion range
    /// </summary>
    public static string CreateProportionFilter(decimal? minPercentage, decimal? maxPercentage)
    {
        var filters = new List<string>();

        if (minPercentage.HasValue)
        {
            var minProportion = PercentageToProportion(minPercentage.Value);
            filters.Add($"participationFees/any(fee: fee/relativeValue/proportion ge {minProportion.ToString(CultureInfo.InvariantCulture)})");
        }

        if (maxPercentage.HasValue)
        {
            var maxProportion = PercentageToProportion(maxPercentage.Value);
            filters.Add($"participationFees/any(fee: fee/relativeValue/proportion le {maxProportion.ToString(CultureInfo.InvariantCulture)})");
        }

        return string.Join(" and ", filters);
    }
}