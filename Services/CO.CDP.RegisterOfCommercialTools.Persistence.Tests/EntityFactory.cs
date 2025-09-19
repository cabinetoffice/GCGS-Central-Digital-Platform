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
}