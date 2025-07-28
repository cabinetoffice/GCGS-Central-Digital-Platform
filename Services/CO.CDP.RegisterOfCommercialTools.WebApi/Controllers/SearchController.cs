using CO.CDP.RegisterOfCommercialTools.WebApi.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
            var sanitizedRequest = SanitizeForLogging(request);
            _logger.LogError(ex, "Error processing search request: {@Request}", sanitizedRequest);
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
    /// <summary>
    /// Recursively sanitizes an object for logging by removing newlines from all string properties.
    /// </summary>
    private static object SanitizeForLogging(object obj)
    {
        if (obj == null)
            return null;

        var type = obj.GetType();
        // If it's a string, sanitize it
        if (obj is string s)
        {
            return s.Replace("\r", "").Replace("\n", "");
        }
        // If it's a value type, return as is
        if (type.IsValueType)
        {
            return obj;
        }
        // If it's an enumerable, sanitize each element
        if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
        {
            var list = new List<object>();
            foreach (var item in enumerable)
            {
                list.Add(SanitizeForLogging(item));
            }
            return list;
        }
        // For complex objects, sanitize each property
        var result = Activator.CreateInstance(type);
        foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;
            var value = prop.GetValue(obj);
            var sanitizedValue = SanitizeForLogging(value);
            prop.SetValue(result, sanitizedValue);
        }
        return result;
    }
}