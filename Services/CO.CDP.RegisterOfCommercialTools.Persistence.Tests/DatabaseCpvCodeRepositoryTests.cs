using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static CO.CDP.RegisterOfCommercialTools.Persistence.Tests.EntityFactory;

namespace CO.CDP.RegisterOfCommercialTools.Persistence.Tests;

public class DatabaseCpvCodeRepositoryTests(PostgreSqlFixture postgreSql)
    : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task GetRootCodesAsync_ReturnsOnlyRootCodesOrderedByCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var rootCodes = GivenMultipleRootCodes();
            var childCode = GivenCpvCode("99100000", "Child code", rootCodes[0].Code, 2);

            await SeedCpvCodes([..rootCodes, childCode]);

            var result = await repository.GetRootCodesAsync();

            result.Should().HaveCount(3);
            result.Should().OnlyContain(c => c.ParentCode == null);
            result.Should().BeInAscendingOrder(c => c.Code);
        });
    }

    [Fact]
    public async Task GetRootCodesAsync_ExcludesInactiveCodes()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var activeRoot = GivenCpvCode("12000001", "Active root");
            var inactiveRoot = GivenCpvCode("34000001", "Inactive root", null, 1, false);

            await SeedCpvCodes([activeRoot, inactiveRoot]);

            var result = await repository.GetRootCodesAsync();

            result.Should().HaveCount(1);
            result.First().Code.Should().Be("12000001");
        });
    }

    [Fact]
    public async Task GetChildrenAsync_ReturnsChildrenOfSpecificParent()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var hierarchy = GivenCpvCodeHierarchy();

            await SeedCpvCodes(hierarchy);

            var result = await repository.GetChildrenAsync(hierarchy[0].Code);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.ParentCode == hierarchy[0].Code);
            result.Should().BeInAscendingOrder(c => c.Code);
        });
    }

    [Fact]
    public async Task GetChildrenAsync_ReturnsEmptyForNonExistentParent()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var hierarchy = GivenCpvCodeHierarchy();

            await SeedCpvCodes(hierarchy);

            var result = await repository.GetChildrenAsync("99999999");

            result.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task SearchAsync_ReturnsCodesMatchingQuery()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>
            {
                GivenCpvCode("13000000", "Construction work"),
                GivenCpvCode("35000000", "Transport equipment"),
                GivenCpvCode("13100000", "Site construction preparation")
            };

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("construction");

            result.Should().HaveCountGreaterOrEqualTo(2);
            result.Should().Contain(c => c.Code == "13000000");
            result.Should().Contain(c => c.Code == "13100000");
            result.Should().NotContain(c => c.Code == "35000000");
        });
    }

    [Fact]
    public async Task SearchAsync_SearchesBothCodeAndDescription()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>
            {
                GivenCpvCode("14000000", "Construction work"),
                GivenCpvCode("36140000", "Vehicles and transport"),
                GivenCpvCode("74000000", "IT services")
            };

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("14");

            result.Should().HaveCount(1);
            result.Select(c => c.Code).Should().Contain(["14000000"]);
        });
    }

    [Fact]
    public async Task SearchAsync_HandlesMultipleSearchTerms()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>
            {
                GivenCpvCode("15000000", "Construction work"),
                GivenCpvCode("15100000", "Site preparation work"),
                GivenCpvCode("37000000", "Transport equipment")
            };

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("site work");

            result.Should().HaveCount(2);
            result.First().Code.Should().Be("15100000");
        });
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyForEmptyQuery()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = GivenMultipleRootCodes();

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("");

            result.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task GetByCodeAsync_ReturnsSpecificCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = GivenMultipleRootCodes();

            await SeedCpvCodes(codes);

            var result = await repository.GetByCodeAsync(codes[0].Code);

            result.Should().NotBeNull();
            result!.Code.Should().Be(codes[0].Code);
            result.GetDescription().Should().Be(codes[0].GetDescription());
        });
    }

    [Fact]
    public async Task GetByCodeAsync_ReturnsNullForNonExistentCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = GivenMultipleRootCodes();

            await SeedCpvCodes(codes);

            var result = await repository.GetByCodeAsync("99999999");

            result.Should().BeNull();
        });
    }

    [Fact]
    public async Task GetByCodeAsync_ReturnsNullForInactiveCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var inactiveCode = GivenCpvCode("16000000", "Inactive code", isActive: false);

            await SeedCpvCodes([inactiveCode]);

            var result = await repository.GetByCodeAsync("16000000");

            result.Should().BeNull();
        });
    }

    [Fact]
    public async Task GetByCodesAsync_ReturnsMultipleCodesOrderedByCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = GivenMultipleRootCodes();

            await SeedCpvCodes(codes);

            var result = await repository.GetByCodesAsync(codes.Select(c => c.Code).ToList());

            result.Should().HaveCount(3);
            result.Should().BeInAscendingOrder(c => c.Code);
        });
    }

    [Fact]
    public async Task GetByCodesAsync_ExcludesNonExistentCodes()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = GivenMultipleRootCodes();

            await SeedCpvCodes(codes);

            var searchCodes = new List<string> { codes[0].Code, "99999999", codes[2].Code };
            var result = await repository.GetByCodesAsync(searchCodes);

            result.Should().HaveCount(2);
            result.Select(c => c.Code).Should().Contain([codes[0].Code, codes[2].Code]);
        });
    }

    [Fact]
    public async Task GetHierarchyAsync_ReturnsFullHierarchyToRoot()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var hierarchy = GivenCpvCodeHierarchy();

            await SeedCpvCodes(hierarchy);

            var result = await repository.GetHierarchyAsync(hierarchy[3].Code);

            result.Should().HaveCount(3);
            result[0].Level.Should().Be(1);
            result[1].Level.Should().Be(2);
            result[2].Level.Should().Be(3);
        });
    }

    [Fact]
    public async Task GetHierarchyAsync_ReturnsOnlyCodeForRootLevel()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var hierarchy = GivenCpvCodeHierarchy();

            await SeedCpvCodes(hierarchy);

            var result = await repository.GetHierarchyAsync(hierarchy[0].Code);

            result.Should().HaveCount(1);
            result.First().Code.Should().Be(hierarchy[0].Code);
        });
    }

    [Fact]
    public async Task GetHierarchyAsync_ReturnsEmptyForNonExistentCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var hierarchy = GivenCpvCodeHierarchy();

            await SeedCpvCodes(hierarchy);

            var result = await repository.GetHierarchyAsync("99999999");

            result.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllActiveCodesOrderedByCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var activeCodes = GivenMultipleRootCodes();
            var inactiveCode = GivenCpvCode("99000000", "Inactive code", isActive: false);

            await SeedCpvCodes([..activeCodes, inactiveCode]);

            var result = await repository.GetAllAsync();

            result.Should().HaveCount(3);
            result.Should().OnlyContain(c => c.IsActive);
            result.Should().BeInAscendingOrder(c => c.Code);
        });
    }

    [Fact]
    public async Task GetDescription_ReturnsWelshWhenCultureIsWelsh()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = GivenMultipleRootCodes();

            await SeedCpvCodes(codes);

            var result = await repository.GetByCodeAsync(codes[0].Code);

            result.Should().NotBeNull();
            result!.GetDescription().Should().Be(codes[0].DescriptionEn);
            result.GetDescription(Culture.Welsh).Should().Be(codes[0].DescriptionCy);
        });
    }

    [Fact]
    public async Task SearchAsync_SearchesBothLanguages()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>
            {
                GivenCpvCode("15000000", "Construction work", null, 1, true, "Gwaith adeiladu"),
                GivenCpvCode("25000000", "Transport services", null, 1, true, "Gwasanaethau cludiant"),
            };

            await SeedCpvCodes(codes);

            var welshResult = await repository.SearchAsync("Gwaith");
            welshResult.Should().HaveCount(2);
            welshResult[0].Code.Should().Be("15000000");

            var englishResult = await repository.SearchAsync("Transport");
            englishResult.Should().HaveCount(1);
            englishResult[0].Code.Should().Be("25000000");
        });
    }

    [Fact]
    public async Task GetRootCodesAsync_SetsHasChildrenPropertyCorrectly()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var rootWithChildren = GivenCpvCode("20000000", "Root with children");
            var rootWithoutChildren = GivenCpvCode("30000000", "Root without children");
            var childCode = GivenCpvCode("20100000", "Child code", rootWithChildren.Code, 2);

            await SeedCpvCodes([rootWithChildren, rootWithoutChildren, childCode]);

            var result = await repository.GetRootCodesAsync();

            var rootWithChildrenResult = result.First(c => c.Code == "20000000");
            var rootWithoutChildrenResult = result.First(c => c.Code == "30000000");

            rootWithChildrenResult.HasChildren.Should().BeTrue();
            rootWithoutChildrenResult.HasChildren.Should().BeFalse();
        });
    }

    [Fact]
    public async Task GetChildrenAsync_SetsHasChildrenPropertyCorrectly()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var root = GivenCpvCode("40000000", "Root");
            var childWithGrandchild = GivenCpvCode("40100000", "Child with grandchild", root.Code, 2);
            var childWithoutGrandchild = GivenCpvCode("40200000", "Child without grandchild", root.Code, 2);
            var grandchild = GivenCpvCode("40110000", "Grandchild", childWithGrandchild.Code, 3);

            await SeedCpvCodes([root, childWithGrandchild, childWithoutGrandchild, grandchild]);

            var result = await repository.GetChildrenAsync(root.Code);

            var childWithGrandchildResult = result.First(c => c.Code == "40100000");
            var childWithoutGrandchildResult = result.First(c => c.Code == "40200000");

            childWithGrandchildResult.HasChildren.Should().BeTrue();
            childWithoutGrandchildResult.HasChildren.Should().BeFalse();
        });
    }

    [Fact]
    public async Task GetByCodeAsync_SetsHasChildrenPropertyCorrectly()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var parentCode = GivenCpvCode("50000000", "Parent");
            var childCode = GivenCpvCode("50100000", "Child", parentCode.Code, 2);

            await SeedCpvCodes([parentCode, childCode]);

            var parentResult = await repository.GetByCodeAsync(parentCode.Code);
            var childResult = await repository.GetByCodeAsync(childCode.Code);

            parentResult.Should().NotBeNull();
            parentResult!.HasChildren.Should().BeTrue();

            childResult.Should().NotBeNull();
            childResult!.HasChildren.Should().BeFalse();
        });
    }

    [Fact]
    public async Task GetByCodesAsync_SetsHasChildrenPropertyCorrectly()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var parentCode = GivenCpvCode("60000000", "Parent");
            var leafCode = GivenCpvCode("70000000", "Leaf");
            var childCode = GivenCpvCode("60100000", "Child", parentCode.Code, 2);

            await SeedCpvCodes([parentCode, leafCode, childCode]);

            var requestedCodes = new List<string> { parentCode.Code, leafCode.Code };
            var result = await repository.GetByCodesAsync(requestedCodes);

            var parentResult = result.First(c => c.Code == "60000000");
            var leafResult = result.First(c => c.Code == "70000000");

            parentResult.HasChildren.Should().BeTrue();
            leafResult.HasChildren.Should().BeFalse();
        });
    }



    [Fact]
    public async Task HasChildren_OnlyCountsActiveChildren()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var parent = GivenCpvCode("85000000", "Parent");
            var activeChild = GivenCpvCode("85100000", "Active child", parent.Code, 2);
            var inactiveChild = GivenCpvCode("85200000", "Inactive child", parent.Code, 2, false);

            await SeedCpvCodes([parent, activeChild, inactiveChild]);

            var result = await repository.GetByCodeAsync(parent.Code);

            result.Should().NotBeNull();
            result!.HasChildren.Should().BeTrue();
        });
    }

    [Fact]
    public async Task HasChildren_ReturnsFalseWhenOnlyInactiveChildren()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var parent = GivenCpvCode("86000000", "Parent");
            var inactiveChild = GivenCpvCode("86100000", "Inactive child", parent.Code, 2, false);

            await SeedCpvCodes([parent, inactiveChild]);

            var result = await repository.GetByCodeAsync(parent.Code);

            result.Should().NotBeNull();
            result!.HasChildren.Should().BeFalse();
        });
    }

    [Fact]
    public async Task SearchAsync_UsesTrigramSimilarityForFuzzyMatching()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>
            {
                GivenCpvCode("16000000", "Construction work and services"),
                GivenCpvCode("17000000", "Medical equipment manufacturing"),
                GivenCpvCode("18000000", "Transport and logistics")
            };

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("construct");

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("16000000");
        });
    }

    [Fact]
    public async Task SearchAsync_HandlesPartialMatches()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>
            {
                GivenCpvCode("19000000", "Manufacturing services"),
                GivenCpvCode("21000000", "Educational services"),
                GivenCpvCode("22000000", "Legal services")
            };

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("manufact");

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("19000000");
        });
    }

    [Fact]
    public async Task SearchAsync_OrdersResultsByRelevanceScore()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>
            {
                GivenCpvCode("23000000", "Construction materials and supplies"),
                GivenCpvCode("24000000", "Construction work"),
                GivenCpvCode("26000000", "Construction services and engineering")
            };

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("construction");

            result.Should().HaveCount(3);
            // Results should be ordered by trigram similarity (best match first)
            result.Should().Contain(c => c.Code == "23000000");
            result.Should().Contain(c => c.Code == "24000000");
            result.Should().Contain(c => c.Code == "26000000");
        });
    }

    [Fact]
    public async Task SearchAsync_SearchesAllLanguagesForBestMatch()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>
            {
                GivenCpvCode("27000000", "Building services", null, 1, true, "Gwasanaethau adeiladu"),
                GivenCpvCode("28000000", "Transport services", null, 1, true, "Gwasanaethau cludiant"),
                GivenCpvCode("29000000", "Medical services", null, 1, true, "Gwasanaethau meddygol")
            };

            await SeedCpvCodes(codes);

            var welshResult = await repository.SearchAsync("adeiladu");
            welshResult.Should().HaveCount(1);
            welshResult[0].Code.Should().Be("27000000");

            var englishResult = await repository.SearchAsync("building");
            englishResult.Should().HaveCount(1);
            englishResult[0].Code.Should().Be("27000000");
        });
    }

    [Fact]
    public async Task SearchAsync_LimitsResultsToTenItems()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>();

            for (int i = 1; i <= 15; i++)
            {
                codes.Add(GivenCpvCode($"{i:D8}", $"Professional service type {i}"));
            }

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("service");

            result.Should().HaveCount(10);
        });
    }

    [Fact]
    public async Task SearchAsync_SetsHasChildrenPropertyCorrectly()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var parentCode = GivenCpvCode("80000000", "Special parent service");
            var leafCode = GivenCpvCode("90000000", "Special leaf service");
            var childCode = GivenCpvCode("80100000", "Child service", parentCode.Code, 2);

            await SeedCpvCodes([parentCode, leafCode, childCode]);

            var result = await repository.SearchAsync("special");

            result.Should().HaveCount(2);
            var parentResult = result.First(c => c.Code == "80000000");
            var leafResult = result.First(c => c.Code == "90000000");

            parentResult.HasChildren.Should().BeTrue();
            leafResult.HasChildren.Should().BeFalse();
        });
    }

    [Fact]
    public async Task SearchAsync_WithMinimumSimilarityThreshold()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = CpvCodeRepository();
            var codes = new List<CpvCode>
            {
                GivenCpvCode("31000000", "Professional consulting services"),
                GivenCpvCode("32000000", "Information technology solutions"),
                GivenCpvCode("33000000", "Financial advisory services")
            };

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("consulting");

            // Should only return results above the 0.1 similarity threshold
            result.Should().HaveCount(1);
            result[0].Code.Should().Be("31000000");
        });
    }


    private DatabaseCpvCodeRepository CpvCodeRepository()
        => new(GetDbContext());

    private RegisterOfCommercialToolsContext? _context;

    private RegisterOfCommercialToolsContext GetDbContext()
    {
        if (_context == null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RegisterOfCommercialToolsContext>();
            optionsBuilder.UseNpgsql(postgreSql.ConnectionString);
            _context = new RegisterOfCommercialToolsContext(optionsBuilder.Options);
            _context.Database.EnsureCreated();

            _context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        }
        return _context;
    }

    private async Task SeedCpvCodes(List<CpvCode> codes)
    {
        var dbContext = GetDbContext();
        dbContext.CpvCodes.AddRange(codes);
        await dbContext.SaveChangesAsync();
    }
}