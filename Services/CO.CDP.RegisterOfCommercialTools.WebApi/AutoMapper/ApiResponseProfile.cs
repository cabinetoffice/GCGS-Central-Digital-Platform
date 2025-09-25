using AutoMapper;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.AutoMapper;

public class ApiResponseProfile : Profile
{
    public ApiResponseProfile()
    {
        CreateMap<CommercialToolApiItem, SearchResultDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TenderIdentifier))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? "Unknown"))
            .ForMember(dest => dest.PublishedDate, opt => opt.MapFrom(src => src.CreatedAt != null ? src.CreatedAt.Value : (DateTime?)null))
            .ForMember(dest => dest.SubmissionDeadline, opt => opt.MapFrom(src => src.TenderPeriod != null ? src.TenderPeriod.EndDate : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DetermineCommercialToolStatus(src.Status)))
            .ForMember(dest => dest.Fees, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.AwardMethod, opt => opt.MapFrom(src => ExtractAwardMethod(src.Techniques)))
            .ForMember(dest => dest.OtherContractingAuthorityCanUse, opt => opt.MapFrom(src => GetOtherContractingAuthorityCanUse(src.Techniques)))
            .ForMember(dest => dest.ContractDates, opt => opt.MapFrom(src => GetContractDates(src.Techniques)))
            .ForMember(dest => dest.CommercialTool, opt => opt.MapFrom(src => GetCommercialTool(src.Techniques)))
            .ForMember(dest => dest.Techniques, opt => opt.MapFrom(src => src.Techniques))
            .ForMember(dest => dest.AdditionalProperties, opt => opt.MapFrom(src => CreateAdditionalProperties(src)));
    }

    private static CommercialToolStatus DetermineCommercialToolStatus(string? status)
    {
        var statusLower = status?.ToLowerInvariant();
        return statusLower switch
        {
            "active" or "planned" => CommercialToolStatus.Active,
            "awarded" or "complete" => CommercialToolStatus.Awarded,
            "cancelled" or "unsuccessful" or "withdrawn" => CommercialToolStatus.Closed,
            _ => CommercialToolStatus.Unknown
        };
    }

    private static string ExtractAwardMethod(TechniquesInfo? techniques)
    {
        if (techniques?.FrameworkAgreement?.Method != null)
        {
            return techniques.FrameworkAgreement.Method switch
            {
                "open" => "With competition",
                "direct" => "Without competition",
                "withoutReopeningCompetition" => "Without competition",
                "withReopeningCompetition" => "With competition",
                "withAndWithoutReopeningCompetition" => "With and without competition",
                _ => techniques.FrameworkAgreement.Method
            };
        }

        return "Unknown";
    }

    private static string GetOtherContractingAuthorityCanUse(TechniquesInfo? techniques)
    {
        var isOpenFramework = techniques?.FrameworkAgreement?.IsOpenFrameworkScheme;

        return isOpenFramework switch
        {
            true => "Yes",
            false => "No",
            null => "Unknown"
        };
    }

    private static string GetContractDates(TechniquesInfo? techniques)
    {
        var startDate = techniques?.FrameworkAgreement?.PeriodStartDate ?? techniques?.FrameworkAgreement?.Period?.StartDate;
        var endDate = techniques?.FrameworkAgreement?.PeriodEndDate ?? techniques?.FrameworkAgreement?.Period?.EndDate;

        if (startDate == null && endDate == null)
        {
            return "Unknown";
        }

        var start = startDate?.ToShortDateString() ?? "Unknown";
        var end = endDate?.ToShortDateString() ?? "Unknown";

        return $"{start} - {end}";
    }

    private static string GetCommercialTool(TechniquesInfo? techniques)
    {
        if (techniques == null)
            return "Unknown";

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

    private static Dictionary<string, string>? CreateAdditionalProperties(CommercialToolApiItem src)
    {
        var properties = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(src.TenderId))
            properties["tenderId"] = src.TenderId;

        if (!string.IsNullOrEmpty(src.TenderIdentifier))
            properties["procurementId"] = src.TenderIdentifier;

        if (src.TenderPeriod?.EndDate != null)
            properties["effectiveEndDateUtc"] = src.TenderPeriod.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");

        return properties.Any() ? properties : null;
    }
}