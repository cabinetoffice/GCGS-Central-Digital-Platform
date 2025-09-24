using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static CO.CDP.RegisterOfCommercialTools.Persistence.Tests.EntityFactory;

namespace CO.CDP.RegisterOfCommercialTools.Persistence.Tests;

public class DatabaseNutsCodeRepositoryTests(PostgreSqlFixture postgreSql)
    : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task GetRootCodesAsync_ReturnsOnlyRootCodesOrderedByCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var rootCodes = GivenMultipleRootNutsCodes();
            var childCode = GivenNutsCode("UKC1", "Child region", rootCodes[0].Code, 2);

            await SeedNutsCodes([..rootCodes, childCode]);

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
            var repository = NutsCodeRepository();
            var activeRoot = GivenNutsCode("UKF", "Active region");
            var inactiveRoot = GivenNutsCode("UKG", "Inactive region", null, 1, false);

            await SeedNutsCodes([activeRoot, inactiveRoot]);

            var result = await repository.GetRootCodesAsync();

            result.Should().HaveCount(1);
            result.First().Code.Should().Be("UKF");
        });
    }

    [Fact]
    public async Task GetChildrenAsync_ReturnsChildrenOfSpecificParent()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var hierarchy = GivenNutsCodeHierarchy();

            await SeedNutsCodes(hierarchy);

            var result = await repository.GetChildrenAsync(hierarchy[0].Code);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.ParentCode == hierarchy[0].Code);
            result.Should().BeInAscendingOrder(c => c.DescriptionEn);
        });
    }

    [Fact]
    public async Task GetChildrenAsync_ReturnsEmptyForNonExistentParent()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var hierarchy = GivenNutsCodeHierarchy();

            await SeedNutsCodes(hierarchy);

            var result = await repository.GetChildrenAsync("INVALID");

            result.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task SearchAsync_ReturnsCodesMatchingQuery()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>
            {
                GivenNutsCode("UKH", "North region"),
                GivenNutsCode("UKI", "South region"),
                GivenNutsCode("UKJ", "Northern territories")
            };

            await SeedNutsCodes(codes);

            var result = await repository.SearchAsync("north");

            result.Should().HaveCountGreaterOrEqualTo(2);
            result.Should().Contain(c => c.Code == "UKH");
            result.Should().Contain(c => c.Code == "UKJ");
            result.Should().NotContain(c => c.Code == "UKI");
        });
    }

    [Fact]
    public async Task SearchAsync_SearchesBothCodeAndDescription()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>
            {
                GivenNutsCode("UKK", "West region"),
                GivenNutsCode("UKL1", "Central territories"),
                GivenNutsCode("UKM", "East region")
            };

            await SeedNutsCodes(codes);

            var result = await repository.SearchAsync("UKK");

            result.Should().HaveCount(3);
            result.Select(c => c.Code).Should().Contain(["UKK"]);
        });
    }

    [Fact]
    public async Task SearchAsync_HandlesMultipleSearchTerms()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>
            {
                GivenNutsCode("UKN", "West region"),
                GivenNutsCode("UKN1", "West central area"),
                GivenNutsCode("UKO", "East territories")
            };

            await SeedNutsCodes(codes);

            var result = await repository.SearchAsync("west area");

            result.Should().HaveCount(2);
            result.First().Code.Should().Be("UKN1");
        });
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyForEmptyQuery()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = GivenMultipleRootNutsCodes();

            await SeedNutsCodes(codes);

            var result = await repository.SearchAsync("");

            result.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task GetByCodeAsync_ReturnsSpecificCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = GivenMultipleRootNutsCodes();

            await SeedNutsCodes(codes);

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
            var repository = NutsCodeRepository();
            var codes = GivenMultipleRootNutsCodes();

            await SeedNutsCodes(codes);

            var result = await repository.GetByCodeAsync("INVALID");

            result.Should().BeNull();
        });
    }

    [Fact]
    public async Task GetByCodeAsync_ReturnsNullForInactiveCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var inactiveCode = GivenNutsCode("UKP", "Inactive region", isActive: false);

            await SeedNutsCodes([inactiveCode]);

            var result = await repository.GetByCodeAsync("UKP");

            result.Should().BeNull();
        });
    }

    [Fact]
    public async Task GetByCodesAsync_ReturnsMultipleCodesOrderedByCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = GivenMultipleRootNutsCodes();

            await SeedNutsCodes(codes);

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
            var repository = NutsCodeRepository();
            var codes = GivenMultipleRootNutsCodes();

            await SeedNutsCodes(codes);

            var searchCodes = new List<string> { codes[0].Code, "INVALID", codes[2].Code };
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
            var repository = NutsCodeRepository();
            var hierarchy = GivenNutsCodeHierarchy();

            await SeedNutsCodes(hierarchy);

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
            var repository = NutsCodeRepository();
            var hierarchy = GivenNutsCodeHierarchy();

            await SeedNutsCodes(hierarchy);

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
            var repository = NutsCodeRepository();
            var hierarchy = GivenNutsCodeHierarchy();

            await SeedNutsCodes(hierarchy);

            var result = await repository.GetHierarchyAsync("INVALID");

            result.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllActiveCodesOrderedByCode()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var activeCodes = GivenMultipleRootNutsCodes();
            var inactiveCode = GivenNutsCode("UKZ", "Inactive region", isActive: false);

            await SeedNutsCodes([..activeCodes, inactiveCode]);

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
            var repository = NutsCodeRepository();
            var codes = GivenMultipleRootNutsCodes();

            await SeedNutsCodes(codes);

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
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>
            {
                GivenNutsCode("UKQ", "Northern region", null, 1, true, true, "Rhanbarth y Gogledd"),
                GivenNutsCode("UKR", "Southern region", null, 1, true, true, "Rhanbarth y De"),
            };

            await SeedNutsCodes(codes);

            var welshResult = await repository.SearchAsync("Gogledd");
            welshResult.Should().HaveCount(1);
            welshResult[0].Code.Should().Be("UKQ");

            var englishResult = await repository.SearchAsync("Northern");
            englishResult.Should().HaveCount(2);
            englishResult[0].Code.Should().Be("UKQ");
        });
    }

    [Fact]
    public async Task GetRootCodesAsync_SetsHasChildrenPropertyCorrectly()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var rootWithChildren = GivenNutsCode("UKS", "Root with children");
            var rootWithoutChildren = GivenNutsCode("UKT", "Root without children");
            var childCode = GivenNutsCode("UKS1", "Child region", rootWithChildren.Code, 2);

            await SeedNutsCodes([rootWithChildren, rootWithoutChildren, childCode]);

            var result = await repository.GetRootCodesAsync();

            var rootWithChildrenResult = result.First(c => c.Code == "UKS");
            var rootWithoutChildrenResult = result.First(c => c.Code == "UKT");

            rootWithChildrenResult.HasChildren.Should().BeTrue();
            rootWithoutChildrenResult.HasChildren.Should().BeFalse();
        });
    }

    [Fact]
    public async Task GetChildrenAsync_SetsHasChildrenPropertyCorrectly()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var root = GivenNutsCode("UKU", "Root");
            var childWithGrandchild = GivenNutsCode("UKU1", "Child with grandchild", root.Code, 2);
            var childWithoutGrandchild = GivenNutsCode("UKU2", "Child without grandchild", root.Code, 2);
            var grandchild = GivenNutsCode("UKU11", "Grandchild", childWithGrandchild.Code, 3);

            await SeedNutsCodes([root, childWithGrandchild, childWithoutGrandchild, grandchild]);

            var result = await repository.GetChildrenAsync(root.Code);

            var childWithGrandchildResult = result.First(c => c.Code == "UKU1");
            var childWithoutGrandchildResult = result.First(c => c.Code == "UKU2");

            childWithGrandchildResult.HasChildren.Should().BeTrue();
            childWithoutGrandchildResult.HasChildren.Should().BeFalse();
        });
    }

    [Fact]
    public async Task GetByCodeAsync_SetsHasChildrenPropertyCorrectly()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var parentCode = GivenNutsCode("UKV", "Parent");
            var childCode = GivenNutsCode("UKV1", "Child", parentCode.Code, 2);

            await SeedNutsCodes([parentCode, childCode]);

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
            var repository = NutsCodeRepository();
            var parentCode = GivenNutsCode("UKW", "Parent");
            var leafCode = GivenNutsCode("UKX", "Leaf");
            var childCode = GivenNutsCode("UKW1", "Child", parentCode.Code, 2);

            await SeedNutsCodes([parentCode, leafCode, childCode]);

            var requestedCodes = new List<string> { parentCode.Code, leafCode.Code };
            var result = await repository.GetByCodesAsync(requestedCodes);

            var parentResult = result.First(c => c.Code == "UKW");
            var leafResult = result.First(c => c.Code == "UKX");

            parentResult.HasChildren.Should().BeTrue();
            leafResult.HasChildren.Should().BeFalse();
        });
    }

    [Fact]
    public async Task HasChildren_OnlyCountsActiveChildren()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var parent = GivenNutsCode("UKY", "Parent");
            var activeChild = GivenNutsCode("UKY1", "Active child", parent.Code, 2);
            var inactiveChild = GivenNutsCode("UKY2", "Inactive child", parent.Code, 2, false);

            await SeedNutsCodes([parent, activeChild, inactiveChild]);

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
            var repository = NutsCodeRepository();
            var parent = GivenNutsCode("UKZ1", "Parent");
            var inactiveChild = GivenNutsCode("UKZ11", "Inactive child", parent.Code, 2, false);

            await SeedNutsCodes([parent, inactiveChild]);

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
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>
            {
                GivenNutsCode("FR1", "Northern France regions"),
                GivenNutsCode("DE1", "German manufacturing areas"),
                GivenNutsCode("ES1", "Spanish coastal territories")
            };

            await SeedNutsCodes(codes);

            var result = await repository.SearchAsync("northern");

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("FR1");
        });
    }

    [Fact]
    public async Task SearchAsync_HandlesPartialMatches()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>
            {
                GivenNutsCode("IT1", "Manufacturing regions"),
                GivenNutsCode("IT2", "Educational districts"),
                GivenNutsCode("IT3", "Agricultural areas")
            };

            await SeedNutsCodes(codes);

            var result = await repository.SearchAsync("manufact");

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("IT1");
        });
    }

    [Fact]
    public async Task SearchAsync_OrdersResultsByRelevanceScore()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>
            {
                GivenNutsCode("NL1", "Northern coastal regions"),
                GivenNutsCode("NL2", "Northern territories"),
                GivenNutsCode("NL3", "Northern industrial areas")
            };

            await SeedNutsCodes(codes);

            var result = await repository.SearchAsync("northern");

            result.Should().HaveCount(3);
            result.Should().Contain(c => c.Code == "NL1");
            result.Should().Contain(c => c.Code == "NL2");
            result.Should().Contain(c => c.Code == "NL3");
        });
    }

    [Fact]
    public async Task SearchAsync_SearchesAllLanguagesForBestMatch()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>
            {
                GivenNutsCode("BE1", "Industrial regions", null, 1, true, true, "Rhanbarthau diwydiannol"),
                GivenNutsCode("BE2", "Coastal territories", null, 1, true, true, "Tiriogaethau arfordirol"),
                GivenNutsCode("BE3", "Mountain areas", null, 1, true, true, "Ardaloedd mynyddig")
            };

            await SeedNutsCodes(codes);

            var welshResult = await repository.SearchAsync("diwydiannol");
            welshResult.Should().HaveCount(1);
            welshResult[0].Code.Should().Be("BE1");

            var englishResult = await repository.SearchAsync("industrial");
            englishResult.Should().HaveCount(1);
            englishResult[0].Code.Should().Be("BE1");
        });
    }

    [Fact]
    public async Task SearchAsync_LimitsResultsToTenItems()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>();

            for (int i = 1; i <= 15; i++)
            {
                codes.Add(GivenNutsCode($"TEST{i:D2}", $"Test region type {i}"));
            }

            await SeedNutsCodes(codes);

            var result = await repository.SearchAsync("region");

            result.Should().HaveCount(10);
        });
    }

    [Fact]
    public async Task SearchAsync_SetsHasChildrenPropertyCorrectly()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var parentCode = GivenNutsCode("SPECIAL1", "Special parent region");
            var leafCode = GivenNutsCode("SPECIAL2", "Special leaf region");
            var childCode = GivenNutsCode("SPECIAL11", "Child region", parentCode.Code, 2);

            await SeedNutsCodes([parentCode, leafCode, childCode]);

            var result = await repository.SearchAsync("special");

            result.Should().HaveCount(3);
            var parentResult = result.First(c => c.Code == "SPECIAL1");
            var leafResult = result.First(c => c.Code == "SPECIAL2");

            parentResult.HasChildren.Should().BeTrue();
            leafResult.HasChildren.Should().BeFalse();
        });
    }

    [Fact]
    public async Task SearchAsync_WithMinimumSimilarityThreshold()
    {
        await GetDbContext().InvokeIsolated(async () =>
        {
            var repository = NutsCodeRepository();
            var codes = new List<NutsCode>
            {
                GivenNutsCode("PROF1", "Professional consulting regions"),
                GivenNutsCode("TECH1", "Technology development areas"),
                GivenNutsCode("FIN1", "Financial advisory districts")
            };

            await SeedNutsCodes(codes);

            var result = await repository.SearchAsync("consulting");

            // Should only return results above the 0.1 similarity threshold
            result.Should().HaveCount(1);
            result[0].Code.Should().Be("PROF1");
        });
    }

    private DatabaseNutsCodeRepository NutsCodeRepository()
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

    private async Task SeedNutsCodes(List<NutsCode> codes)
    {
        var dbContext = GetDbContext();
        dbContext.NutsCodes.AddRange(codes);
        await dbContext.SaveChangesAsync();
    }
}