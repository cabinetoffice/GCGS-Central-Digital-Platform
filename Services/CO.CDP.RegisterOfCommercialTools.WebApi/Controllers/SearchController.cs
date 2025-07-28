using CO.CDP.RegisterOfCommercialTools.WebApi.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResponse>> Search([FromQuery] SearchRequestDto request)
    {
        _logger.LogInformation("Received search request: {@Request}", request);
        try
        {
            var results = await _searchService.Search(request);
            _logger.LogInformation("Search request successful. Found {Count} results.", results.TotalCount);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing search request: {@Request}", request);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SearchResultDto>> GetById(string id)
    {
        _logger.LogInformation("Received GetById request for ID: {Id}", id);
        try
        {
            var result = await _searchService.GetById(id);
            if (result == null)
            {
                _logger.LogWarning("No search result found for ID: {Id}", id);
                return NotFound();
            }
            _logger.LogInformation("Search result found for ID: {Id}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetById request for ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}