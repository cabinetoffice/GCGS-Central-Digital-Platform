using AutoMapper;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;
using CO.CDP.RegisterOfCommercialTools.WebApi.Helpers;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.AutoMapper;

public class ApiResponseProfile : Profile
{
    public ApiResponseProfile()
    {
        CreateMap<CommercialToolApiItem, SearchResultDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Tender != null ? src.Tender.Title : "Unknown"))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Buyer != null ? src.Buyer.Name ?? "Unknown" : "Unknown"))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => GenerateFindTenderUrl(src.Ocid)))
            .ForMember(dest => dest.PublishedDate, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.SubmissionDeadline, opt => opt.MapFrom(src => src.Tender != null && src.Tender.TenderPeriod != null && src.Tender.TenderPeriod.EndDate != null ? src.Tender.TenderPeriod.EndDate.Value.ToString("dd MMMM yyyy") : "Unknown"))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DetermineCommercialToolStatus(src.Tender != null ? src.Tender.Status : null)))
            .ForMember(dest => dest.MaximumFee, opt => opt.MapFrom(src => GetMaximumFee(src.Tender)))
            .ForMember(dest => dest.AwardMethod, opt => opt.MapFrom(src => ExtractAwardMethod(src.Tender)))
            .ForMember(dest => dest.OtherContractingAuthorityCanUse, opt => opt.MapFrom(src => GetOtherContractingAuthorityCanUse(src.Tender)))
            .ForMember(dest => dest.ContractDates, opt => opt.MapFrom(src => GetContractDates(src)))
            .ForMember(dest => dest.CommercialTool, opt => opt.MapFrom(src => GetCommercialTool(src.Tender)))
            .ForMember(dest => dest.Techniques, opt => opt.MapFrom(src => MapTechniques(src.Tender)))
            .ForMember(dest => dest.AdditionalProperties, opt => opt.MapFrom(src => CreateAdditionalProperties(src)));
    }

    private static CommercialToolStatus DetermineCommercialToolStatus(string? status)
    {
        var statusLower = status?.ToLowerInvariant();
        return statusLower switch
        {
            "active" => CommercialToolStatus.Active,
            "planning" or "planned" => CommercialToolStatus.Upcoming,
            "awarded" or "complete" => CommercialToolStatus.Awarded,
            "cancelled" or "unsuccessful" or "withdrawn" => CommercialToolStatus.Expired,
            _ => CommercialToolStatus.Unknown
        };
    }

    private static string GetMaximumFee(CommercialToolTender? tender)
    {
        if (tender?.ParticipationFees == null || !tender.ParticipationFees.Any())
            return "0%";

        var proportions = tender.ParticipationFees
            .Select(f => f.RelativeValueProportion ?? 0)
            .ToList();

        var maxPercentage = proportions.Max() * 100;
        return $"{maxPercentage:0.##}%";
    }

    private static string ExtractAwardMethod(CommercialToolTender? tender)
    {
        if (tender?.Techniques?.FrameworkAgreement?.Method != null)
        {
            return tender.Techniques.FrameworkAgreement.Method switch
            {
                "open" => "With competition",
                "direct" => "Without competition",
                "withoutReopeningCompetition" => "Without competition",
                "withReopeningCompetition" => "With competition",
                "withAndWithoutReopeningCompetition" => "With and without competition",
                _ => "Unknown"
            };
        }

        return "Unknown";
    }

    private static string GetOtherContractingAuthorityCanUse(CommercialToolTender? tender)
    {
        var frameworkType = tender?.Techniques?.FrameworkAgreement?.Type?.ToLowerInvariant();

        return frameworkType switch
        {
            "open" => "Yes",
            "closed" => "No",
            _ => "Unknown"
        };
    }

    private static string GetContractDates(CommercialToolApiItem item)
    {
        var tender = item.Tender;

        // Try to get end date (priority 1: frameworkAgreement)
        var endDate = tender?.Techniques?.FrameworkAgreement?.PeriodEndDate
            ?? tender?.Techniques?.FrameworkAgreement?.Period?.EndDate;

        // Fallback 2: awards contractPeriod endDate
        if (endDate == null && item.Awards != null && item.Awards.Any())
        {
            endDate = item.Awards.FirstOrDefault()?.ContractPeriod?.EndDate;
        }

        // Try to get start date (priority 1: frameworkAgreement)
        var startDate = tender?.Techniques?.FrameworkAgreement?.PeriodStartDate
            ?? tender?.Techniques?.FrameworkAgreement?.Period?.StartDate;

        // Fallback 2: standstill period end date + 1 day
        if (startDate == null && item.Awards != null && item.Awards.Any())
        {
            var standstillEndDate = item.Awards.FirstOrDefault()?.StandstillPeriod?.EndDate;
            if (standstillEndDate.HasValue)
            {
                startDate = standstillEndDate.Value.AddDays(1);
            }
        }

        // Fallback 3: award period end date + 8 working days
        if (startDate == null && tender?.AwardPeriod?.EndDate != null)
        {
            startDate = DateHelper.AddWorkingDays(tender.AwardPeriod.EndDate.Value, 8);
        }

        if (startDate == null || endDate == null)
        {
            return "Unknown";
        }

        return $"{startDate.Value:dd MMMM yyyy} to {endDate.Value:dd MMMM yyyy}";
    }

    private static string GetCommercialTool(CommercialToolTender? tender)
    {
        if (tender?.Techniques == null)
            return "Unknown";

        var techniques = tender.Techniques;
        var tags = new List<string>();

        if (techniques.HasFrameworkAgreement == true)
            tags.Add("Framework agreement");

        if (techniques.FrameworkAgreement?.IsOpenFrameworkScheme == true)
            tags.Add("Open framework scheme");

        if (techniques.HasDynamicPurchasingSystem == true)
            tags.Add("Dynamic purchasing system");

        if (techniques.HasElectronicAuction == true)
            tags.Add("Electronic auction");

        return tags.Any() ? string.Join(", ", tags) : "Unknown";
    }

    private static TechniquesInfo? MapTechniques(CommercialToolTender? tender)
    {
        if (tender?.Techniques == null)
            return null;

        var techniques = tender.Techniques;
        var mappedTechniques = new TechniquesInfo
        {
            HasFrameworkAgreement = techniques.HasFrameworkAgreement,
            HasDynamicPurchasingSystem = techniques.HasDynamicPurchasingSystem,
            HasElectronicAuction = techniques.HasElectronicAuction
        };

        if (techniques.FrameworkAgreement != null)
        {
            mappedTechniques.FrameworkAgreement = new FrameworkAgreement
            {
                Method = techniques.FrameworkAgreement.Method,
                IsOpenFrameworkScheme = techniques.FrameworkAgreement.IsOpenFrameworkScheme,
                PeriodStartDate = techniques.FrameworkAgreement.PeriodStartDate,
                PeriodEndDate = techniques.FrameworkAgreement.PeriodEndDate
            };

            if (techniques.FrameworkAgreement.Period != null)
            {
                mappedTechniques.FrameworkAgreement.Period = new FrameworkAgreementPeriod
                {
                    StartDate = techniques.FrameworkAgreement.Period.StartDate,
                    EndDate = techniques.FrameworkAgreement.Period.EndDate
                };
            }
        }

        return mappedTechniques;
    }

    private static Dictionary<string, string>? CreateAdditionalProperties(CommercialToolApiItem src)
    {
        var properties = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(src.Id))
            properties["id"] = src.Id;

        if (!string.IsNullOrEmpty(src.Ocid))
            properties["ocid"] = src.Ocid;

        if (!string.IsNullOrEmpty(src.Tender?.TenderId))
            properties["tenderId"] = src.Tender.TenderId;

        if (!string.IsNullOrEmpty(src.Ocid))
            properties["procurementId"] = src.Ocid;

        if (!string.IsNullOrEmpty(src.Buyer?.Name))
            properties["buyerName"] = src.Buyer.Name;

        if (src.Tender?.TenderPeriod?.EndDate != null)
            properties["effectiveEndDateUtc"] = src.Tender.TenderPeriod.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");

        var buyerParty = src.Parties?.FirstOrDefault(p => p.Roles?.Contains("buyer") == true);
        if (buyerParty?.Locations?.FirstOrDefault()?.PhysicalAddress != null)
        {
            var address = buyerParty.Locations.First().PhysicalAddress;
            var addressParts = new List<string>();

            if (!string.IsNullOrEmpty(address?.AddressLine1)) addressParts.Add(address.AddressLine1);
            if (!string.IsNullOrEmpty(address?.AddressLine2)) addressParts.Add(address.AddressLine2);
            if (!string.IsNullOrEmpty(address?.Locality)) addressParts.Add(address.Locality);
            if (!string.IsNullOrEmpty(address?.PostalCode)) addressParts.Add(address.PostalCode);

            if (addressParts.Any())
                properties["buyerAddress"] = string.Join(", ", addressParts);
        }

        return properties.Any() ? properties : null;
    }

    private static string? GenerateFindTenderUrl(string? ocid)
    {
        return !string.IsNullOrEmpty(ocid)
            ? $"https://www.find-tender.service.gov.uk/procurement/{ocid}"
            : null;
    }
}