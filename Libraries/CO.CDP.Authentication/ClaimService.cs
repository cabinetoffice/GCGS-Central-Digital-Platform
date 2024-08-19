using CO.CDP.OrganisationInformation;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace CO.CDP.Authentication;
public class ClaimService(IHttpContextAccessor httpContextAccessor) : IClaimService
{
    public string? GetUserUrn()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }

    public bool HaveAccessToOrganisation(Guid oragnisationId)
    {
        var tenantlookup = GetTenantLookup();
        if (tenantlookup == null) return false;

        return tenantlookup.Tenants.SelectMany(t => t.Organisations).Any(o => o.Id == oragnisationId);
    }

    public int? GetOrganisationId()
    {
        if (int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst("org")?.Value, out int result))
        {
            return result;
        }
        return null;
    }

    private TenantLookup? GetTenantLookup()
    {
        var tenantClaim = httpContextAccessor.HttpContext?.User?.FindFirst("ten")?.Value;
        if (tenantClaim == null) return null;
        try
        {
            var decompressd = Encoding.UTF8.GetString(Decompress(Convert.FromBase64String(tenantClaim)));
            var tenantLookup = JsonSerializer.Deserialize<TenantLookup>(decompressd);
            return tenantLookup;
        }
        catch
        {
            return null;
        }
    }

    private static byte[] Decompress(byte[] compressedData)
    {
        using var uncompressedStream = new MemoryStream();

        using (var compressedStream = new MemoryStream(compressedData))
        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress, false))
        {
            gzipStream.CopyTo(uncompressedStream);
        }

        return uncompressedStream.ToArray();
    }
}