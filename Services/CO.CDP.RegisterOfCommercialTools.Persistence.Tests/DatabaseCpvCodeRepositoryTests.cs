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
                GivenCpvCode("73000000", "IT consulting services"),
                GivenCpvCode("13100000", "Site construction preparation")
            };

            await SeedCpvCodes(codes);

            var result = await repository.SearchAsync("construction");

            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.GetDescription(Culture.English).ToLower().Contains("construction"));
            result.Should().BeInAscendingOrder(c => c.Code);
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

            result.Should().HaveCount(2);
            result.Select(c => c.Code).Should().Contain(["14000000", "36140000"]);
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

            result.Should().HaveCount(1);
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

            var result = await repository.GetHierarchyAsync(hierarchy[3].Code); // grandchild

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

            var result = await repository.GetHierarchyAsync(hierarchy[0].Code); // root

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
            welshResult.Should().HaveCount(1);
            welshResult[0].Code.Should().Be("15000000");

            var englishResult = await repository.SearchAsync("Transport");
            englishResult.Should().HaveCount(1);
            englishResult[0].Code.Should().Be("25000000");
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