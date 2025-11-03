using CO.CDP.RegisterOfCommercialTools.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NutsCodeController(INutsCodeRepository repository) : ControllerBase
{
    [HttpGet("root")]
    public async Task<ActionResult<List<NutsCode>>> GetRootCodes(Culture culture = Culture.English)
    {
        var codes = await repository.GetRootCodesAsync(culture);
        return Ok(codes);
    }

    [HttpGet("{parentCode}/children")]
    public async Task<ActionResult<List<NutsCode>>> GetChildren(string parentCode, Culture culture = Culture.English)
    {
        var codes = await repository.GetChildrenAsync(parentCode, culture);
        return Ok(codes);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<NutsCode>>> Search([FromQuery] string query, Culture culture = Culture.English)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter is required");
        }

        var codes = await repository.SearchAsync(query, culture);
        return Ok(codes);
    }

    [HttpPost("lookup")]
    public async Task<ActionResult<List<NutsCode>>> GetByCodes([FromBody] List<string> codes)
    {
        if (codes.Count == 0)
        {
            return BadRequest("Codes list is required");
        }

        var nutsCodes = await repository.GetByCodesAsync(codes);
        return Ok(nutsCodes);
    }

    [HttpGet("{code}/hierarchy")]
    public async Task<ActionResult<List<NutsCode>>> GetHierarchy(string code)
    {
        var hierarchy = await repository.GetHierarchyAsync(code);
        return Ok(hierarchy);
    }
}