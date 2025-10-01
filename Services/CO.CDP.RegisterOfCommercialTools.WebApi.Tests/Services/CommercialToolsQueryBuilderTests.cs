using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using FluentAssertions;
using System.Globalization;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Services;

public class CommercialToolsQueryBuilderTests
{
    private const string BaseUrl = "https://api.example.com/tenders";

    [Fact]
    public void Constructor_ShouldNotAddAnyFilters()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.Build(BaseUrl);

        result.Should().Be(BaseUrl);
    }

    [Fact]
    public void WithKeywords_WhenKeywordsProvided_ShouldAddSearchParameterAndFieldFilters()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords("test keyword").Build(BaseUrl);

        result.Should().Contain("$search=test%20keyword");
        result.Should().Contain("filter[tender.name]=test%20keyword");
        result.Should().Contain("filter[tender.techniques.frameworkAgreement.description]=test%20keyword");
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
    public void WithKeywords_WhenSpaceSeparatedWords_ShouldPassThroughForAnyWordSearch()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords("radio televisions").Build(BaseUrl);

        result.Should().Contain("$search=radio%20televisions");
    }

    [Fact]
    public void WithKeywords_WhenPlusOperatorUsed_ShouldConvertToAndOperator()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords("administration + defence").Build(BaseUrl);

        result.Should().Contain("$search=administration%20AND%20defence");
    }

    [Fact]
    public void WithKeywords_WhenQuotedPhrase_ShouldPassThroughForExactMatch()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords("\"market research\"").Build(BaseUrl);

        result.Should().Contain("$search=%22market%20research%22");
    }

    [Fact]
    public void WithKeywords_WhenMultiplePlusOperators_ShouldConvertAllToAnd()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords("technology + innovation + digital").Build(BaseUrl);

        result.Should().Contain("$search=technology%20AND%20innovation%20AND%20digital");
    }

    [Fact]
    public void OnlyOpenFrameworks_WhenTrue_ShouldSetFilterToTrue()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.OnlyOpenFrameworks().Build(BaseUrl);

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
    public void WithStatus_WhenActiveProvided_ShouldAddODataFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithStatus("Active").Build(BaseUrl);

        result.Should().Contain("$filter=tender%2Fstatus%20eq%20%27active%27");
    }

    [Fact]
    public void WithStatus_WhenUpcomingProvided_ShouldAddODataFilterWithOrCondition()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithStatus("upcoming").Build(BaseUrl);

        result.Should().Contain("$filter=%28tender%2Fstatus%20eq%20%27planned%27%20or%20tender%2Fstatus%20eq%20%27planning%27%29");
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

        var expectedProportion = (123.45m / 100).ToString(CultureInfo.InvariantCulture);
        result.Should().Contain($"filter[tender.participationFees.relativeValue.proportion.from]={expectedProportion}");
    }

    [Fact]
    public void FeeTo_ShouldAddFeeToFilterWithInvariantCulture()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.FeeTo(999.99m).Build(BaseUrl);

        var expectedProportion = (999.99m / 100).ToString(CultureInfo.InvariantCulture);
        result.Should().Contain($"filter[tender.participationFees.relativeValue.proportion.to]={expectedProportion}");
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
    public void WithSkip_ShouldAddSkipParameter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithSkip(40).Build(BaseUrl);

        result.Should().Contain("$skip=40");
    }

    [Fact]
    public void WithTop_ShouldAddTopParameter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithTop(25).Build(BaseUrl);

        result.Should().Contain("$top=25");
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
        result.Should().Contain("$filter=tender%2Fstatus%20eq%20%27active%27");
    }

    [Fact]
    public void Build_ShouldUrlEncodeParameterValues()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords("test with spaces").Build(BaseUrl);

        result.Should().Contain("$search=test%20with%20spaces");
        result.Should().Contain("filter[tender.name]=test%20with%20spaces");
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

        result1.Should().NotContain("$search=test");
        result1.Should().NotContain("filter[tender.name]=test");
        result1.Should().NotContain("filter[tender.status]=Active");

        result2.Should().Contain("$search=test");
        result2.Should().Contain("filter[tender.name]=test");
        result2.Should().NotContain("filter[tender.status]=Active");

        result3.Should().Contain("$search=test");
        result3.Should().Contain("filter[tender.name]=test");
        result3.Should().Contain("$filter=tender%2Fstatus%20eq%20%27active%27");
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
            .WithSkip(25)
            .WithTop(25)
            .Build(BaseUrl);

        result.Should().StartWith(BaseUrl);
        result.Should().Contain("$search=IT%20services");
        result.Should().Contain("filter[tender.name]=IT%20services");
        result.Should().Contain("$filter=tender%2Fstatus%20eq%20%27active%27");
        result.Should().Contain("filter[tender.participationFees.relativeValue.proportion.from]=1.005");
        result.Should().Contain("filter[tender.participationFees.relativeValue.proportion.to]=9.9999");
        result.Should().Contain("filter[tender.tenderPeriod.endDate.from]=2025-01-01");
        result.Should().Contain("filter[tender.tenderPeriod.endDate.to]=2025-12-31");
        result.Should().Contain("filter[tender.lots.contractPeriod.startDate.from]=2025-06-01");
        result.Should().Contain("filter[tender.lots.contractPeriod.startDate.to]=2025-06-30");
        result.Should().Contain("$top=25");
        result.Should().Contain("$skip=25");
    }


    [Fact]
    public void WithFrameworkType_WhenTypeProvided_ShouldAddFrameworkTypeFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithFrameworkType("open").Build(BaseUrl);

        result.Should().Contain("filter[tender.techniques.frameworkAgreement.type]=open");
    }

    [Fact]
    public void WithFrameworkType_WhenEmpty_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithFrameworkType("");

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithBuyerClassificationRestrictions_WhenUtilities_ShouldAddUtilitiesFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithBuyerClassificationRestrictions("utilities").Build(BaseUrl);

        result.Should().Contain("filter[tender.techniques.frameworkAgreement.buyerClassificationRestrictions.id]=utilities");
    }

    [Fact]
    public void WithBuyerClassificationRestrictions_WhenEmpty_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithBuyerClassificationRestrictions("");

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void ExcludeBuyerClassificationRestrictions_WhenUtilities_ShouldAddNotEqualsFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.ExcludeBuyerClassificationRestrictions("utilities").Build(BaseUrl);

        result.Should().Contain("filter[tender.techniques.frameworkAgreement.buyerClassificationRestrictions.id ne]=utilities");
    }

    [Fact]
    public void ExcludeBuyerClassificationRestrictions_WhenEmpty_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.ExcludeBuyerClassificationRestrictions("");

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithAwardMethod_WhenWithCompetition_ShouldMapToOpenFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithAwardMethod("with-competition").Build(BaseUrl);

        result.Should().Contain("filter[tender.awardCriteria.method]=open");
    }

    [Fact]
    public void WithAwardMethod_WhenWithoutCompetition_ShouldMapToDirectFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithAwardMethod("without-competition").Build(BaseUrl);

        result.Should().Contain("filter[tender.awardCriteria.method]=direct");
    }

    [Fact]
    public void WithAwardMethod_WhenEmpty_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithAwardMethod("");

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithAwardMethod_WhenNull_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithAwardMethod(null!);

        result.Should().BeSameAs(builder);
    }
}