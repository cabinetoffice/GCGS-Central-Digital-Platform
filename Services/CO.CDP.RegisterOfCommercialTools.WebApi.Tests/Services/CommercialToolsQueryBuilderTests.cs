using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
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
    public void WithKeywords_WhenKeywordsProvided_ShouldAddFilterWithContains()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords(["test"], KeywordSearchMode.Any).Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("contains");
        result.Should().Contain("tender%2Ftitle");
    }

    [Fact]
    public void WithKeywords_WhenKeywordsEmpty_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords([], KeywordSearchMode.Any);

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithKeywords_WhenKeywordsNull_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords(null, KeywordSearchMode.Any);

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithKeywords_WhenMultipleTermsWithAnyMode_ShouldUseOrLogic()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords(["radio", "televisions"], KeywordSearchMode.Any).Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("%20or%20");
    }

    [Fact]
    public void WithKeywords_WhenMultipleTermsWithAllMode_ShouldUseAndLogic()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords(["administration", "defence"], KeywordSearchMode.All).Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("%20and%20");
    }

    [Fact]
    public void WithKeywords_WhenExactMode_ShouldSearchForExactPhrase()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords(["market research"], KeywordSearchMode.Exact).Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("contains");
        result.Should().Contain("market%20research");
    }

    [Fact]
    public void WithKeywords_WhenMultipleTermsWithAllMode_ShouldUseAndLogicForAll()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithKeywords(["technology", "innovation", "digital"], KeywordSearchMode.All).Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("%20and%20");
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
    public void WithLocation_WhenRegionProvided_ShouldAddLocationFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithLocation("UKN06").Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("tender%2Fitems%2Fany");
        result.Should().Contain("deliveryAddresses%2Fany");
        result.Should().Contain("region%20eq%20%27UKN06%27");
    }

    [Fact]
    public void WithLocation_WhenEmpty_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithLocation("");

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithLocation_WhenNull_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithLocation(null!);

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithCpv_WhenCpvProvided_ShouldAddCpvFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithCpv("12345678").Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("tender%2Fclassification%2Fscheme%20eq%20%27CPV%27");
        result.Should().Contain("tender%2Fclassification%2FclassificationId%20eq%20%2712345678%27");
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

        var result = builder.WithKeywords(["test"], KeywordSearchMode.Any).Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("contains");
    }

    [Fact]
    public void ChainedOperations_ShouldReturnImmutableInstances()
    {
        var builder1 = new CommercialToolsQueryBuilder();
        var builder2 = builder1.WithKeywords(["test"], KeywordSearchMode.Any);
        var builder3 = builder2.WithStatus("Active");

        builder1.Should().NotBeSameAs(builder2);
        builder2.Should().NotBeSameAs(builder3);

        var result1 = builder1.Build(BaseUrl);
        var result2 = builder2.Build(BaseUrl);
        var result3 = builder3.Build(BaseUrl);

        result1.Should().NotContain("contains");
        result1.Should().NotContain("tender%2Fstatus%20eq%20%27active%27");

        result2.Should().Contain("contains");
        result2.Should().NotContain("tender%2Fstatus%20eq%20%27active%27");

        result3.Should().Contain("contains");
        result3.Should().Contain("tender%2Fstatus%20eq%20%27active%27");
    }

    [Fact]
    public void ComplexQuery_ShouldBuildCorrectUrl()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder
            .WithKeywords(["IT", "services"], KeywordSearchMode.Any)
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
        result.Should().Contain("contains");
        result.Should().Contain("tender%2Fstatus%20eq%20%27active%27");
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

        result.Should().Contain("$filter=tender%2Ftechniques%2FframeworkAgreement%2Ftype%20eq%20%27open%27");
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

        result.Should().Contain("$filter=");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27open%27");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27withReopeningCompetition%27");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27withAndWithoutReopeningCompetition%27");
    }

    [Fact]
    public void WithAwardMethod_WhenWithoutCompetition_ShouldMapToDirectFilter()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithAwardMethod("without-competition").Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27direct%27");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27withoutReopeningCompetition%27");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27withAndWithoutReopeningCompetition%27");
    }

    [Fact]
    public void WithAwardMethod_WhenWithAndWithoutCompetition_ShouldMapToAllMethods()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithAwardMethod("with-and-without-competition").Build(BaseUrl);

        result.Should().Contain("$filter=");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27open%27");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27withReopeningCompetition%27");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27withAndWithoutReopeningCompetition%27");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27direct%27");
        result.Should().Contain("tender%2Ftechniques%2FframeworkAgreement%2Fmethod%20eq%20%27withoutReopeningCompetition%27");
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

    [Fact]
    public void WithOrderBy_WhenAtoZ_ShouldAddOrderByAscending()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithOrderBy("a-z").Build(BaseUrl);

        result.Should().Contain("$orderby=tender%2Ftitle%20asc");
    }

    [Fact]
    public void WithOrderBy_WhenZtoA_ShouldAddOrderByDescending()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithOrderBy("z-a").Build(BaseUrl);

        result.Should().Contain("$orderby=tender%2Ftitle%20desc");
    }

    [Fact]
    public void WithOrderBy_WhenRelevance_ShouldAddRelevanceOrderBy()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithOrderBy("relevance").Build(BaseUrl);

        result.Should().Contain("$orderby=tender%2FcreatedDate%20desc");
    }

    [Fact]
    public void WithOrderBy_WhenEmpty_ShouldReturnSameInstance()
    {
        var builder = new CommercialToolsQueryBuilder();

        var result = builder.WithOrderBy("");

        result.Should().BeSameAs(builder);
    }
}