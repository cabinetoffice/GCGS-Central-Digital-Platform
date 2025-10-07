namespace CO.CDP.RegisterOfCommercialTools.Persistence.Tests;

public static class EntityFactory
{
    private static readonly Random Random = new();

    public static CpvCode GivenCpvCode(
        string? code = null,
        string? descriptionEn = null,
        string? parentCode = null,
        int level = 1,
        bool isActive = true,
        string? descriptionCy = null)
    {
        var theCode = code ?? GenerateRandomCpvCode(level);
        var theDescriptionEn = descriptionEn ?? $"Description for {theCode}";
        var theDescriptionCy = descriptionCy ?? $"Disgrifiad ar gyfer {theCode}";

        return new CpvCode
        {
            Code = theCode,
            DescriptionEn = theDescriptionEn,
            DescriptionCy = theDescriptionCy,
            ParentCode = parentCode,
            Level = level,
            IsActive = isActive,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }

    public static List<CpvCode> GivenCpvCodeHierarchy()
    {
        var baseId = Random.Next(10, 99);
        var rootCode = GivenCpvCode($"{baseId}000000", "Construction work", null, 1, true, "Gwaith adeiladu");
        var childCode1 = GivenCpvCode($"{baseId}100000", "Site preparation work", $"{baseId}000000", 2, true, "Gwaith paratoi safle");
        var childCode2 = GivenCpvCode($"{baseId}200000", "Building construction work", $"{baseId}000000", 2, true, "Gwaith adeiladu adeiladau");
        var grandChildCode = GivenCpvCode($"{baseId}110000", "Site clearance work", $"{baseId}100000", 3, true, "Gwaith clirio safle");

        return [rootCode, childCode1, childCode2, grandChildCode];
    }

    public static List<CpvCode> GivenMultipleRootCodes()
    {
        var id1 = Random.Next(10, 50);
        var id2 = Random.Next(51, 80);
        var id3 = Random.Next(81, 99);

        var construction = GivenCpvCode($"{id1}000000", "Construction work", null, 1, true, "Gwaith adeiladu");
        var transport = GivenCpvCode($"{id2}000000", "Transport equipment and auxiliary products", null, 1, true, "Offer cludiant a chynhyrchion ategol");
        var services = GivenCpvCode($"{id3}000000", "IT services: consulting, software development", null, 1, true, "Gwasanaethau TG: ymgynghori, datblygu meddalwedd");

        return [construction, transport, services];
    }

    public static NutsCode GivenNutsCode(
        string? code = null,
        string? descriptionEn = null,
        string? parentCode = null,
        int level = 1,
        bool isActive = true,
        bool isSelectable = true,
        string? descriptionCy = null)
    {
        var theCode = code ?? GenerateRandomNutsCode(level);
        var theDescriptionEn = descriptionEn ?? $"Description for {theCode}";
        var theDescriptionCy = descriptionCy ?? $"Disgrifiad ar gyfer {theCode}";

        return new NutsCode
        {
            Code = theCode,
            DescriptionEn = theDescriptionEn,
            DescriptionCy = theDescriptionCy,
            ParentCode = parentCode,
            Level = level,
            IsActive = isActive,
            IsSelectable = isSelectable,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }

    public static List<NutsCode> GivenNutsCodeHierarchy()
    {
        var region = "UKC";
        var rootCode = GivenNutsCode($"{region}", "North East (England)", null, 1, true, true, "Gogledd Ddwyrain (Lloegr)");
        var childCode1 = GivenNutsCode($"{region}1", "Tees Valley and Durham", $"{region}", 2, true, true, "Cwm Tees a Durham");
        var childCode2 = GivenNutsCode($"{region}2", "Northumberland and Tyne and Wear", $"{region}", 2, true, true, "Northumberland a Tyne a Wear");
        var grandChildCode = GivenNutsCode($"{region}11", "Hartlepool and Stockton-on-Tees", $"{region}1", 3, true, true, "Hartlepool a Stockton-on-Tees");

        return [rootCode, childCode1, childCode2, grandChildCode];
    }

    public static List<NutsCode> GivenMultipleRootNutsCodes()
    {
        var northEast = GivenNutsCode("UKC", "North East (England)", null, 1, true, true, "Gogledd Ddwyrain (Lloegr)");
        var northWest = GivenNutsCode("UKD", "North West (England)", null, 1, true, true, "Gogledd Orllewin (Lloegr)");
        var yorkshire = GivenNutsCode("UKE", "Yorkshire and The Humber", null,1, true, true, "Yorkshire a'r Humber");

        return [northEast, northWest, yorkshire];
    }

    private static string GenerateRandomCpvCode(int level)
    {
        var digits = level switch
        {
            1 => $"{Random.Next(10, 99)}000000",
            2 => $"{Random.Next(10, 99)}{Random.Next(100, 999)}000",
            3 => $"{Random.Next(10, 99)}{Random.Next(100, 999)}{Random.Next(100, 999)}",
            _ => $"{Random.Next(10, 99)}000000"
        };
        return digits;
    }

    private static string GenerateRandomNutsCode(int level)
    {
        var letters = new[] { "UK", "FR", "DE", "ES", "IT" };
        var baseRegion = letters[Random.Next(letters.Length)];

        return level switch
        {
            1 => $"{baseRegion}{(char)('A' + Random.Next(0, 26))}",
            2 => $"{baseRegion}{(char)('A' + Random.Next(0, 26))}{Random.Next(1, 9)}",
            3 => $"{baseRegion}{(char)('A' + Random.Next(0, 26))}{Random.Next(1, 9)}{Random.Next(1, 9)}",
            _ => $"{baseRegion}{(char)('A' + Random.Next(0, 26))}"
        };
    }
}