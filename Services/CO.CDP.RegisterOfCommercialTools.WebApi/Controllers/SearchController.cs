using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SearchResponse>> Search([FromQuery] SearchRequestDto request)
    {
        var results = await searchService.Search(request);
        return Ok(results);
    }
}