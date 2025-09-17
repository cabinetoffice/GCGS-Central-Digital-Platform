using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CO.CDP.RegisterOfCommercialTools.App.Services;

namespace CO.CDP.RegisterOfCommercialTools.App.Controllers;

[ApiController]
[Route("api/cpv")]
[AllowAnonymous]
public class CpvCodesController(ICpvCodeService cpvCodeService) : ControllerBase
{
    [HttpGet("roots")]
    public async Task<IActionResult> GetRootCpvCodes()
    {
        var result = await cpvCodeService.GetRootCpvCodesAsync();
        return Ok(result);
    }

    [HttpGet("children/{parentCode}")]
    public async Task<IActionResult> GetChildren(string parentCode)
    {
        if (string.IsNullOrWhiteSpace(parentCode)) return BadRequest("parentCode is required");
        var result = await cpvCodeService.GetChildrenAsync(parentCode);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("query is required");
        var result = await cpvCodeService.SearchAsync(query);
        return Ok(result);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return BadRequest("code is required");
        var result = await cpvCodeService.GetByCodeAsync(code);
        if (result == null) return NotFound();
        return Ok(result);
    }
}

