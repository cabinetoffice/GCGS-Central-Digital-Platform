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
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => GetTitle(src)))
            .ForMember(dest => dest.BuyerName, opt => opt.MapFrom(src => GetBuyerName(src)))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => GenerateFindTenderUrl(src.Ocid)))
            .ForMember(dest => dest.SubmissionDeadline, opt => opt.MapFrom(src => GetSubmissionDeadline(src)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DetermineCommercialToolStatus(src.Tender != null ? src.Tender.Status : null)))
            .ForMember(dest => dest.MaximumFee, opt => opt.MapFrom(src => GetMaximumFee(src.Tender)))
            .ForMember(dest => dest.AwardMethod, opt => opt.MapFrom(src => ExtractAwardMethod(src)))
            .ForMember(dest => dest.OtherContractingAuthorityCanUse, opt => opt.MapFrom(src => GetOtherContractingAuthorityCanUse(src)))
            .ForMember(dest => dest.ContractDates, opt => opt.MapFrom(src => GetContractDates(src)))
            .ForMember(dest => dest.CommercialTool, opt => opt.MapFrom(src => GetCommercialTool(src)))
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
            "complete" => CommercialToolStatus.Awarded,
            "cancelled" or "unsuccessful" or "withdrawn" => CommercialToolStatus.Cancelled,
            _ => CommercialToolStatus.Unknown
        };
    }

    private static string GetFallbackText(CommercialToolStatus status)
    {
        return status == CommercialToolStatus.Upcoming || status == CommercialToolStatus.Active ? "TBC" : "Unknown";
    }

    private static string GetTitle(CommercialToolApiItem src)
    {
        if (src.Tender?.Title != null)
        {
            return src.Tender.Title;
        }

        var status = DetermineCommercialToolStatus(src.Tender?.Status);
        return GetFallbackText(status);
    }

    private static string GetBuyerName(CommercialToolApiItem src)
    {
        return src.Buyer?.Name ?? "Unknown";
    }

    private static string GetSubmissionDeadline(CommercialToolApiItem src)
    {
        if (src.Tender?.TenderPeriod?.EndDate != null)
        {
            return src.Tender.TenderPeriod.EndDate.Value.ToString("dd MMMM yyyy");
        }

        var status = DetermineCommercialToolStatus(src.Tender?.Status);
        return GetFallbackText(status);
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

    private static string ExtractAwardMethod(CommercialToolApiItem src)
    {
        if (src.Tender?.Techniques?.FrameworkAgreement?.Method != null)
        {
            return src.Tender.Techniques.FrameworkAgreement.Method switch
            {
                "open" => "With competition",
                "direct" => "Without competition",
                "withoutReopeningCompetition" => "Without competition",
                "withReopeningCompetition" => "With competition",
                "withAndWithoutReopeningCompetition" => "With and without competition",
                _ => GetFallbackText(DetermineCommercialToolStatus(src.Tender?.Status))
            };
        }

        var status = DetermineCommercialToolStatus(src.Tender?.Status);
        return GetFallbackText(status);
    }

    private static string GetOtherContractingAuthorityCanUse(CommercialToolApiItem src)
    {
        var frameworkType = src.Tender?.Techniques?.FrameworkAgreement?.Type?.ToLowerInvariant();

        if (frameworkType != null)
        {
            return frameworkType switch
            {
                "open" => "Yes",
                "closed" => "No",
                _ => GetFallbackText(DetermineCommercialToolStatus(src.Tender?.Status))
            };
        }

        var status = DetermineCommercialToolStatus(src.Tender?.Status);
        return GetFallbackText(status);
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

        // Fallback 3: tender lots contractPeriod endDate
        if (endDate == null && tender?.Lots != null && tender.Lots.Any())
        {
            endDate = tender.Lots.FirstOrDefault()?.ContractPeriod?.EndDate;
        }

        // Fallback 4: contracts period endDate
        if (endDate == null && item.Contracts != null && item.Contracts.Any())
        {
            endDate = item.Contracts.FirstOrDefault()?.Period?.EndDate;
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

        // Fallback 3: tender lots contractPeriod startDate
        if (startDate == null && tender?.Lots != null && tender.Lots.Any())
        {
            startDate = tender.Lots.FirstOrDefault()?.ContractPeriod?.StartDate;
        }

        // Fallback 4: contracts period startDate
        if (startDate == null && item.Contracts != null && item.Contracts.Any())
        {
            startDate = item.Contracts.FirstOrDefault()?.Period?.StartDate;
        }

        // Fallback 5: award period end date + 8 working days
        if (startDate == null && tender?.AwardPeriod?.EndDate != null)
        {
            startDate = DateHelper.AddWorkingDays(tender.AwardPeriod.EndDate.Value, 8);
        }

        if (startDate == null || endDate == null)
        {
            var status = DetermineCommercialToolStatus(item.Tender?.Status);
            return GetFallbackText(status);
        }

        return $"{startDate.Value:dd MMMM yyyy} to {endDate.Value:dd MMMM yyyy}";
    }

    private static string GetCommercialTool(CommercialToolApiItem src)
    {
        if (src.Tender?.Techniques == null)
        {
            var status = DetermineCommercialToolStatus(src.Tender?.Status);
            return GetFallbackText(status);
        }

        var techniques = src.Tender.Techniques;
        var tags = new List<string>();

        if (techniques.HasFrameworkAgreement == true)
            tags.Add("Framework agreement");

        if (techniques.FrameworkAgreement?.IsOpenFrameworkScheme == true)
            tags.Add("Open framework scheme");

        if (techniques.HasDynamicPurchasingSystem == true)
            tags.Add("Dynamic purchasing system");

        if (techniques.HasElectronicAuction == true)
            tags.Add("Electronic auction");

        if (tags.Any())
        {
            return string.Join(", ", tags);
        }

        var fallbackStatus = DetermineCommercialToolStatus(src.Tender?.Status);
        return GetFallbackText(fallbackStatus);
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

        if (!string.IsNullOrEmpty(src.Buyer?.Name))
            properties["buyerName"] = src.Buyer.Name;

        if (src.Tender?.TenderPeriod?.EndDate != null)
            properties["effectiveEndDateUtc"] = src.Tender.TenderPeriod.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");

        var buyerParty = src.Parties?.FirstOrDefault(p => p.Roles?.Contains("buyer") == true);

        return properties.Any() ? properties : null;
    }

    private static string? GenerateFindTenderUrl(string? ocid)
    {
        return !string.IsNullOrEmpty(ocid)
            ? $"https://www.find-tender.service.gov.uk/procurement/{ocid}"
            : null;
    }
}