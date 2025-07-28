using CO.CDP.RegisterOfCommercialTools.WebApi.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResponse>> Search([FromQuery] SearchRequestDto request)
    {
        var results = await _searchService.Search(request);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SearchResultDto>> GetById(string id)
    {
        var result = await _searchService.GetById(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}