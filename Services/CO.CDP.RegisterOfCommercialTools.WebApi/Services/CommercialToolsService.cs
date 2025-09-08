using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;
using System.Text.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class CommercialToolsService : ICommercialToolsService
{
    private readonly HttpClient _httpClient;

    public CommercialToolsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("x-api-key", configuration.GetValue<string>("ODataApi:ApiKey"));
    }


    public async Task<(IEnumerable<SearchResultDto> results, int totalCount)> SearchCommercialToolsWithCount(string queryUrl)
    {
        var response = await _httpClient.GetAsync(queryUrl);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return ([], 0);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"ODI API returned {response.StatusCode}: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


        var rawJsonResponse = JsonSerializer.Deserialize<JsonElement>(content, options);

        var totalCount = 0;
        if (rawJsonResponse.TryGetProperty("@odata.count", out var odataCount))
        {
            totalCount = odataCount.GetInt32();
            Console.WriteLine($"Found @odata.count: {totalCount}");
        }
        else if (rawJsonResponse.TryGetProperty("metadata", out var metadata) &&
                 metadata.TryGetProperty("count", out var metadataCount))
        {
            totalCount = metadataCount.GetInt32();
            Console.WriteLine($"Found metadata.count: {totalCount}");
        }
        else
        {
            Console.WriteLine("No @odata.count or metadata.count property found in response");
        }

        var tenderInfoList = new List<TenderInfoDto>();

        // Expect 'data' array only; do not use 'value'. If missing, return no results.
        if (!rawJsonResponse.TryGetProperty("data", out var dataArray))
        {
            Console.WriteLine("No 'data' property found in response; returning no results.");
            return (Enumerable.Empty<SearchResultDto>(), totalCount);
        }

        Console.WriteLine($"Found 'data' array with {dataArray.GetArrayLength()} items");
        foreach (var tenderElement in dataArray.EnumerateArray())
        {
            var tenderInfo = new TenderInfoDto
            {
                Name = tenderElement.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "" : "",
                Status = tenderElement.TryGetProperty("status", out var statusElement) ? statusElement.GetString() ?? "" : "",
                PublishedDate = ExtractPublishedDate(tenderElement),
                AdditionalProperties = ExtractAdditionalProperties(tenderElement),
                ParticipationFees = ExtractParticipationFees(tenderElement),
                Techniques = ExtractTechniquesInfo(tenderElement)
            };
            Console.WriteLine($"Processing tender: {tenderInfo.Name}, Status: {tenderInfo.Status}");
            tenderInfoList.Add(tenderInfo);
        }

        var processedResults = new List<SearchResultDto>();
        foreach (var tenderInfo in tenderInfoList)
        {
            var id = GetResultId(tenderInfo);

            var dto = new SearchResultDto
            {
                Id = id,
                Title = tenderInfo.Name,
                Description = tenderInfo.AdditionalProperties?.GetValueOrDefault("procuringEntity") ?? "Unknown",
                PublishedDate = tenderInfo.PublishedDate?.DateTime ?? DateTime.UtcNow,
                SubmissionDeadline = tenderInfo.AdditionalProperties?.ContainsKey("effectiveEndDateUtc") == true
                    ? DateTime.TryParse(tenderInfo.AdditionalProperties["effectiveEndDateUtc"], out var endDate)
                        ? endDate
                        : null
                    : null,
                Fees = FeeConverter.GetMaxFeePercentage(tenderInfo.ParticipationFees) ?? 0,
                AwardMethod = ExtractAwardMethod(tenderInfo),
                Status = DetermineCommercialToolStatus(tenderInfo.Status),
                OtherContractingAuthorityCanUse = GetOtherContractingAuthorityCanUse(tenderInfo),
                ContractDates = GetContractDates(tenderInfo),
                Techniques = tenderInfo.Techniques,
                AdditionalProperties = tenderInfo.AdditionalProperties
            };

            processedResults.Add(dto);
        }

        Console.WriteLine($"Processed {processedResults.Count} results, total count: {totalCount}");
        return (processedResults, totalCount);
    }

    public async Task<int> GetCommercialToolsCount(string queryUrl)
    {
        try
        {
            var countUrl = AddCountParameter(queryUrl);
            var response = await _httpClient.GetAsync(countUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return 0;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var result = JsonSerializer.Deserialize<JsonElement>(content, options);

            if (result.TryGetProperty("@odata.count", out var odataCount))
            {
                return odataCount.GetInt32();
            }

            if (result.TryGetProperty("totalCount", out var totalCountProperty))
            {
                return totalCountProperty.GetInt32();
            }

            if (result.TryGetProperty("meta", out var meta) &&
                meta.TryGetProperty("count", out var metaCount))
            {
                return metaCount.GetInt32();
            }

            var rawResults = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(content, options);
            return rawResults?.Count() ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private static string AddCountParameter(string queryUrl)
    {
        var separator = queryUrl.Contains("?") ? "&" : "?";
        return $"{queryUrl}{separator}$count=true";
    }


    private static DateTimeOffset? ExtractPublishedDate(JsonElement tenderElement)
    {
        if (tenderElement.TryGetProperty("documents", out var documentsElement) &&
            documentsElement.ValueKind == JsonValueKind.Array)
        {
            var earliestDate = DateTimeOffset.MaxValue;
            var foundDate = false;

            foreach (var document in documentsElement.EnumerateArray())
            {
                if (document.TryGetProperty("datePublished", out var datePublishedElement))
                {
                    if (datePublishedElement.TryGetProperty("dateTime", out var dateTimeElement) &&
                        dateTimeElement.TryGetProperty("value", out var valueElement))
                    {
                        if (DateTimeOffset.TryParse(valueElement.GetString(), out var publishedDate))
                        {
                            if (publishedDate < earliestDate)
                            {
                                earliestDate = publishedDate;
                                foundDate = true;
                            }
                        }
                    }
                    else if (DateTimeOffset.TryParse(datePublishedElement.GetString(), out var simpleDate))
                    {
                        if (simpleDate < earliestDate)
                        {
                            earliestDate = simpleDate;
                            foundDate = true;
                        }
                    }
                }
            }

            return foundDate ? earliestDate : null;
        }

        return null;
    }

    private static Dictionary<string, string>? ExtractAdditionalProperties(JsonElement tenderElement)
    {
        var properties = new Dictionary<string, string>();

        if (tenderElement.TryGetProperty("identifier", out var identifierElement))
        {
            if (identifierElement.TryGetProperty("id", out var idElement))
                properties["tenderId"] = idElement.GetString() ?? "";
            if (identifierElement.TryGetProperty("uri", out var uriElement))
                properties["uri"] = uriElement.GetString() ?? "";
        }

        if (tenderElement.TryGetProperty("additionalProperties", out var additionalPropsElement))
        {
            if (additionalPropsElement.TryGetProperty("procurementId", out var procurementIdElement))
                properties["procurementId"] = procurementIdElement.GetString() ?? "";
            if (additionalPropsElement.TryGetProperty("tenderId", out var tenderIdElement))
                properties["tenderId"] = tenderIdElement.GetString() ?? "";
            if (additionalPropsElement.TryGetProperty("uri", out var uriElement))
                properties["uri"] = uriElement.GetString() ?? "";
            if (additionalPropsElement.TryGetProperty("procuringEntity", out var procuringEntityElement))
                properties["procuringEntity"] = procuringEntityElement.GetString() ?? "";
            if (additionalPropsElement.TryGetProperty("effectiveEndDateUtc", out var effectiveEndElement))
                properties["effectiveEndDateUtc"] = effectiveEndElement.GetString() ?? "";
        }


        return properties.Any() ? properties : null;
    }

    private static List<ParticipationFee>? ExtractParticipationFees(JsonElement tenderElement)
    {
        if (tenderElement.TryGetProperty("participationFees", out var feesElement) &&
            feesElement.ValueKind == JsonValueKind.Array)
        {
            var fees = new List<ParticipationFee>();

            foreach (var feeElement in feesElement.EnumerateArray())
            {
                if (feeElement.TryGetProperty("relativeValue", out var relativeValueElement) &&
                    relativeValueElement.TryGetProperty("proportion", out var proportionElement))
                {
                    if (decimal.TryParse(proportionElement.GetRawText(), out var proportion))
                    {
                        fees.Add(new ParticipationFee
                        {
                            RelativeValue = new RelativeValue
                            {
                                Proportion = proportion
                            }
                        });
                    }
                }
            }

            return fees.Any() ? fees : null;
        }

        return null;
    }

    private static string ExtractAwardMethod(TenderInfoDto tenderInfo)
    {
        if (tenderInfo.Techniques?.FrameworkAgreement?.Method != null)
        {
            return tenderInfo.Techniques.FrameworkAgreement.Method switch
            {
                "open" => "With competition",
                "direct" => "Without competition",
                _ => tenderInfo.Techniques.FrameworkAgreement.Method
            };
        }

        return "Unknown";
    }


    private static TechniquesInfo? ExtractTechniquesInfo(JsonElement tenderElement)
    {
        if (tenderElement.TryGetProperty("tender", out var tenderInnerElement) &&
            tenderInnerElement.TryGetProperty("techniques", out var techniquesElement))
        {
            return new TechniquesInfo
            {
                HasFrameworkAgreement = techniquesElement.TryGetProperty("hasFrameworkAgreement", out var hasFrameworkElement)
                    ? hasFrameworkElement.GetBoolean()
                    : null,
                HasDynamicPurchasingSystem = techniquesElement.TryGetProperty("hasDynamicPurchasingSystem", out var hasDynamicElement)
                    ? hasDynamicElement.GetBoolean()
                    : null,
                HasElectronicAuction = techniquesElement.TryGetProperty("hasElectronicAuction", out var hasElectronicElement)
                    ? hasElectronicElement.GetBoolean()
                    : null,
                FrameworkAgreement = ExtractFrameworkAgreementFromTechniques(techniquesElement)
            };
        }
        return null;
    }


    private static FrameworkAgreement? ExtractFrameworkAgreementFromTechniques(JsonElement techniquesElement)
    {
        if (techniquesElement.TryGetProperty("frameworkAgreement", out var frameworkElement))
        {
            var framework = new FrameworkAgreement
            {
                Type = frameworkElement.TryGetProperty("type", out var typeElement)
                    ? typeElement.GetString()
                    : null,
                Description = frameworkElement.TryGetProperty("description", out var descElement)
                    ? descElement.GetString()
                    : null,
                Method = frameworkElement.TryGetProperty("method", out var methodElement)
                    ? methodElement.GetString()
                    : null,
                IsOpenFrameworkScheme = frameworkElement.TryGetProperty("isOpenFrameworkScheme", out var isOpenElement)
                    ? isOpenElement.GetBoolean()
                    : null,
                Period = frameworkElement.TryGetProperty("period", out var periodElement)
                    ? new FrameworkAgreementPeriod
                    {
                        StartDate = periodElement.TryGetProperty("startDate", out var startElement)
                            ? DateTime.TryParse(startElement.GetString(), out var startDate) ? startDate : null
                            : null,
                        EndDate = periodElement.TryGetProperty("endDate", out var endElement)
                            ? DateTime.TryParse(endElement.GetString(), out var endDate) ? endDate : null
                            : null
                    }
                    : null
            };

            if (frameworkElement.TryGetProperty("buyerClassificationRestrictions", out var restrictionsElement) &&
                restrictionsElement.ValueKind == JsonValueKind.Array)
            {
                var restrictions = new List<BuyerClassificationRestriction>();
                foreach (var restrictionElement in restrictionsElement.EnumerateArray())
                {
                    restrictions.Add(new BuyerClassificationRestriction
                    {
                        Id = restrictionElement.TryGetProperty("id", out var idElement)
                            ? idElement.GetString()
                            : null,
                        Description = restrictionElement.TryGetProperty("description", out var restrictionDescElement)
                            ? restrictionDescElement.GetString()
                            : null
                    });
                }
                framework.BuyerClassificationRestrictions = restrictions.Any() ? restrictions : null;
            }

            return framework;
        }
        return null;
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

    private static string GetResultId(TenderInfoDto tenderInfo)
    {
        return tenderInfo.AdditionalProperties?.GetValueOrDefault("procurementId") 
               ?? tenderInfo.AdditionalProperties?.GetValueOrDefault("tenderId") 
               ?? Guid.NewGuid().ToString();
    }

    private static string GetOtherContractingAuthorityCanUse(TenderInfoDto tenderInfo)
    {
        var isOpenFramework = tenderInfo.Techniques?.FrameworkAgreement?.IsOpenFrameworkScheme;
        
        return isOpenFramework switch
        {
            true => "Yes",
            false => "No",
            null => "Unknown"
        };
    }

    private static string GetContractDates(TenderInfoDto tenderInfo)
    {
        var startDate = tenderInfo.Techniques?.FrameworkAgreement?.Period?.StartDate;
        var endDate = tenderInfo.Techniques?.FrameworkAgreement?.Period?.EndDate;
        
        if (startDate == null && endDate == null)
        {
            return "Unknown";
        }
        
        var start = startDate?.ToShortDateString() ?? "Unknown";
        var end = endDate?.ToShortDateString() ?? "Unknown";
        
        return $"{start} - {end}";
    }
}