using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using CO.CDP.WebApi.Foundation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController(ISearchService searchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SearchResponse>> Search([FromQuery] SearchRequestDto request)
    {
        var result = await searchService.Search(request);

        return result.Match<ActionResult<SearchResponse>>(
            error => error switch
            {
                ClientError clientError => StatusCode((int)clientError.StatusCode, new { message = clientError.Message }),
                ServerError serverError => StatusCode((int)serverError.StatusCode, new { message = serverError.Message }),
                NetworkError networkError => StatusCode(502, new { message = "External service unavailable", details = networkError.Message }),
                DeserialisationError deserialisationError => StatusCode(502, new { message = "External service returned invalid data", details = deserialisationError.Message }),
                _ => StatusCode(500, new { message = "An unexpected error occurred", details = error.Message })
            },
            searchResponse => Ok(searchResponse)
        );
    }
}