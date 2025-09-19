using CO.CDP.RegisterOfCommercialTools.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CpvCodeController(ICpvCodeRepository repository) : ControllerBase
{
    [HttpGet("root")]
    public async Task<ActionResult<List<CpvCode>>> GetRootCodes(Culture culture = Culture.English)
    {
        var codes = await repository.GetRootCodesAsync(culture);
        return Ok(codes);
    }

    [HttpGet("{parentCode}/children")]
    public async Task<ActionResult<List<CpvCode>>> GetChildren(string parentCode, Culture culture = Culture.English)
    {
        var codes = await repository.GetChildrenAsync(parentCode, culture);
        return Ok(codes);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<CpvCode>>> Search([FromQuery] string query, Culture culture = Culture.English)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter is required");
        }

        var codes = await repository.SearchAsync(query, culture);
        return Ok(codes);
    }

    [HttpPost("lookup")]
    public async Task<ActionResult<List<CpvCode>>> GetByCodes([FromBody] List<string> codes)
    {
        if (codes.Count == 0)
        {
            return BadRequest("Codes list is required");
        }

        var cpvCodes = await repository.GetByCodesAsync(codes);
        return Ok(cpvCodes);
    }

    [HttpGet("{code}/hierarchy")]
    public async Task<ActionResult<List<CpvCode>>> GetHierarchy(string code)
    {
        var hierarchy = await repository.GetHierarchyAsync(code);
        return Ok(hierarchy);
    }
}