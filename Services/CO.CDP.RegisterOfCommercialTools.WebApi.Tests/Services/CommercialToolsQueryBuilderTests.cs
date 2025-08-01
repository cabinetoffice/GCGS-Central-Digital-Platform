using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using FluentAssertions;
using System.Globalization;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Services;

public class CommercialToolsQueryBuilderTests
{
    private const string BaseUrl = "https://api.example.com/tenders";

    [Fact]
    public void Constructor_ShouldSetDefaultOpenFrameworkFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.Build(BaseUrl);

        result.Should().Contain("filter[tender.techniques.frameworkAgreement.isOpenFrameworkScheme]=true");
    }

    [Fact]
    public void WithKeywords_WhenKeywordsProvided_ShouldAddAllKeywordFilters()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords("test keyword").Build(BaseUrl);

        result.Should().Contain("filter[tender.title]=test%20keyword");
        result.Should().Contain("filter[tender.description]=test%20keyword");
        result.Should().Contain("filter[parties.identifier.id]=test%20keyword");
        result.Should().Contain("filter[parties.name]=test%20keyword");
    }

    [Fact]
    public void WithKeywords_WhenKeywordsEmpty_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords("");

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithKeywords_WhenKeywordsNull_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords(null!);

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void OnlyOpenFrameworks_WhenTrue_ShouldSetFilterToTrue()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.OnlyOpenFrameworks(true).Build(BaseUrl);

        result.Should().Contain("filter[tender.techniques.frameworkAgreement.isOpenFrameworkScheme]=true");
    }

    [Fact]
    public void OnlyOpenFrameworks_WhenFalse_ShouldSetFilterToFalse()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.OnlyOpenFrameworks(false).Build(BaseUrl);

        result.Should().Contain("filter[tender.techniques.frameworkAgreement.isOpenFrameworkScheme]=false");
    }

    [Fact]
    public void WithStatus_WhenStatusProvided_ShouldAddStatusFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithStatus("Active").Build(BaseUrl);

        result.Should().Contain("filter[tender.status]=Active");
    }

    [Fact]
    public void WithStatus_WhenStatusEmpty_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithStatus("");

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void FeeFrom_ShouldAddFeeFromFilterWithInvariantCulture()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.FeeFrom(123.45m).Build(BaseUrl);

        result.Should().Contain($"filter[tender.participationFees.relativeValue.proportion.value.from]={123.45m.ToString(CultureInfo.InvariantCulture)}");
    }

    [Fact]
    public void FeeTo_ShouldAddFeeToFilterWithInvariantCulture()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.FeeTo(999.99m).Build(BaseUrl);

        result.Should().Contain($"filter[tender.participationFees.relativeValue.proportion.value.to]={999.99m.ToString(CultureInfo.InvariantCulture)}");
    }

    [Fact]
    public void SubmissionDeadlineFrom_ShouldAddSubmissionDeadlineFromFilter()
    {
        var builder = new CommercialToolsQueryBuilder();
        var date = new DateTime(2025, 1, 15);

        var result = builder.SubmissionDeadlineFrom(date).Build(BaseUrl);

        result.Should().Contain("filter[tender.tenderPeriod.endDate.from]=2025-01-15");
    }

    [Fact]
    public void SubmissionDeadlineTo_ShouldAddSubmissionDeadlineToFilter()
    {
        var builder = new CommercialToolsQueryBuilder();
        var date = new DateTime(2025, 12, 31);

        var result = builder.SubmissionDeadlineTo(date).Build(BaseUrl);

        result.Should().Contain("filter[tender.tenderPeriod.endDate.to]=2025-12-31");
    }

    [Fact]
    public void ContractStartDateFrom_ShouldAddContractStartDateFromFilter()
    {
        var builder = new CommercialToolsQueryBuilder();
        var date = new DateTime(2025, 6, 1);

        var result = builder.ContractStartDateFrom(date).Build(BaseUrl);

        result.Should().Contain("filter[tender.lots.contractPeriod.startDate.from]=2025-06-01");
    }

    [Fact]
    public void ContractStartDateTo_ShouldAddContractStartDateToFilter()
    {
        var builder = new CommercialToolsQueryBuilder();
        var date = new DateTime(2025, 6, 30);

        var result = builder.ContractStartDateTo(date).Build(BaseUrl);

        result.Should().Contain("filter[tender.lots.contractPeriod.startDate.to]=2025-06-30");
    }

    [Fact]
    public void ContractEndDateFrom_ShouldAddContractEndDateFromFilter()
    {
        var builder = new CommercialToolsQueryBuilder();
        var date = new DateTime(2026, 5, 31);

        var result = builder.ContractEndDateFrom(date).Build(BaseUrl);

        result.Should().Contain("filter[tender.lots.contractPeriod.endDate.from]=2026-05-31");
    }

    [Fact]
    public void ContractEndDateTo_ShouldAddContractEndDateToFilter()
    {
        var builder = new CommercialToolsQueryBuilder();
        var date = new DateTime(2026, 12, 31);

        var result = builder.ContractEndDateTo(date).Build(BaseUrl);

        result.Should().Contain("filter[tender.lots.contractPeriod.endDate.to]=2026-12-31");
    }

    [Fact]
    public void WithPageSize_ShouldAddPageSizeFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithPageSize(20).Build(BaseUrl);

        result.Should().Contain("page[size]=20");
    }

    [Fact]
    public void WithPageNumber_ShouldAddPageNumberFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithPageNumber(3).Build(BaseUrl);

        result.Should().Contain("page[number]=3");
    }

    [Fact]
    public void ReservedParticipation_WhenModeProvided_ShouldAddReservedParticipationFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.ReservedParticipation("sme").Build(BaseUrl);

        result.Should().Contain("filter[tender.otherRequirements.reservedParticipation]=sme");
    }

    [Fact]
    public void ContractLocation_WhenRegionProvided_ShouldAddContractLocationFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.ContractLocation("London").Build(BaseUrl);

        result.Should().Contain("filter[tender.items.deliveryAddresses.region]=London");
    }

    [Fact]
    public void WithCpv_WhenCpvProvided_ShouldAddCpvFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithCpv("12345678").Build(BaseUrl);

        result.Should().Contain("filter[tender.items.additionalClassifications.id]=12345678");
    }

    [Fact]
    public void Build_WhenBaseUrlIsNull_ShouldThrowArgumentNullException()
    {
        var builder = new CommercialToolsQueryBuilder();

        var action = () => builder.Build(null!);

        action.Should().Throw<ArgumentNullException>().WithParameterName("baseUrl");
    }

    [Fact]
    public void Build_WhenBaseUrlIsEmpty_ShouldThrowArgumentNullException()
    {
        var builder = new CommercialToolsQueryBuilder();

        var action = () => builder.Build("");

        action.Should().Throw<ArgumentNullException>().WithParameterName("baseUrl");
    }

    [Fact]
    public void Build_WhenBaseUrlAlreadyHasQuery_ShouldAppendWithAmpersand()
    {
        var builder = new CommercialToolsQueryBuilder();
        var baseUrlWithQuery = "https://api.example.com/tenders?existing=param";

        var result = builder.WithStatus("Active").Build(baseUrlWithQuery);

        result.Should().StartWith(baseUrlWithQuery);
        result.Should().Contain("&filter[tender.status]=Active");
    }

    [Fact]
    public void Build_ShouldUrlEncodeParameterValues()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords("test with spaces").Build(BaseUrl);

        result.Should().Contain("filter[tender.title]=test%20with%20spaces");
    }

    [Fact]
    public void ChainedOperations_ShouldReturnImmutableInstances()
    {
        var builder1 = new CommercialToolsQueryBuilder();
        var builder2 = builder1.WithKeywords("test");
        var builder3 = builder2.WithStatus("Active");

        builder1.Should().NotBeSameAs(builder2);
        builder2.Should().NotBeSameAs(builder3);

        var result1 = builder1.Build(BaseUrl);
        var result2 = builder2.Build(BaseUrl);
        var result3 = builder3.Build(BaseUrl);

        result1.Should().NotContain("filter[tender.title]=test");
        result1.Should().NotContain("filter[tender.status]=Active");

        result2.Should().Contain("filter[tender.title]=test");
        result2.Should().NotContain("filter[tender.status]=Active");

        result3.Should().Contain("filter[tender.title]=test");
        result3.Should().Contain("filter[tender.status]=Active");
    }

    [Fact]
    public void ComplexQuery_ShouldBuildCorrectUrl()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder
            .WithKeywords("IT services")
            .WithStatus("Active")
            .FeeFrom(100.50m)
            .FeeTo(999.99m)
            .SubmissionDeadlineFrom(new DateTime(2025, 1, 1))
            .SubmissionDeadlineTo(new DateTime(2025, 12, 31))
            .ContractStartDateFrom(new DateTime(2025, 6, 1))
            .ContractStartDateTo(new DateTime(2025, 6, 30))
            .WithPageSize(25)
            .WithPageNumber(2)
            .Build(BaseUrl);

        result.Should().StartWith(BaseUrl);
        result.Should().Contain("filter[tender.title]=IT%20services");
        result.Should().Contain("filter[tender.status]=Active");
        result.Should().Contain("filter[tender.participationFees.relativeValue.proportion.value.from]=100.5");
        result.Should().Contain("filter[tender.participationFees.relativeValue.proportion.value.to]=999.99");
        result.Should().Contain("filter[tender.tenderPeriod.endDate.from]=2025-01-01");
        result.Should().Contain("filter[tender.tenderPeriod.endDate.to]=2025-12-31");
        result.Should().Contain("filter[tender.lots.contractPeriod.startDate.from]=2025-06-01");
        result.Should().Contain("filter[tender.lots.contractPeriod.startDate.to]=2025-06-30");
        result.Should().Contain("page[size]=25");
        result.Should().Contain("page[number]=2");
    }
}