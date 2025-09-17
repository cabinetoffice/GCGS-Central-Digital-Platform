using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Controllers;

[AllowAnonymous]
public class CpvCodesController(ICpvCodeService cpvCodeService) : Controller
{

    [HttpGet("/cpv/tree-fragment")]
    public async Task<IActionResult> GetTreeFragment([FromQuery] string[]? selectedCodes = null, [FromQuery] string? expandedCode = null)
    {
        try
        {
            var rootCodes = await cpvCodeService.GetRootCpvCodesAsync();
            var viewModel = new CpvSelectionViewModel
            {
                RootCodes = rootCodes,
                SelectedCodes = selectedCodes?.ToList() ?? new List<string>(),
                ExpandedCode = expandedCode
            };

            if (!string.IsNullOrEmpty(expandedCode))
            {
                var children = await cpvCodeService.GetChildrenAsync(expandedCode);
                var expandedNode = FindAndExpandNode(rootCodes, expandedCode, children);
                if (expandedNode != null)
                {
                    viewModel = viewModel.WithRootCodes(rootCodes);
                }
            }

            return PartialView("_CpvTreeFragment", viewModel);
        }
        catch (Exception)
        {
            var errorModel = new CpvSelectionViewModel().WithError("There is a problem loading CPV codes. Try refreshing the page.");
            return PartialView("_CpvTreeFragment", errorModel);
        }
    }

    [HttpGet("/cpv/search-fragment")]
    public async Task<IActionResult> GetSearchFragment([FromQuery] string q, [FromQuery] string[]? selectedCodes = null)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            var emptyModel = new CpvSelectionViewModel
            {
                SelectedCodes = selectedCodes?.ToList() ?? new List<string>(),
                SearchQuery = q
            };
            return PartialView("_CpvSearchFragment", emptyModel);
        }

        try
        {
            var searchResults = await cpvCodeService.SearchAsync(q);
            var viewModel = new CpvSelectionViewModel
            {
                SearchResults = searchResults,
                SelectedCodes = selectedCodes?.ToList() ?? new List<string>(),
                SearchQuery = q
            };

            return PartialView("_CpvSearchFragment", viewModel);
        }
        catch (Exception)
        {
            var errorModel = new CpvSelectionViewModel().WithError("There is a problem with search. Try again or browse the categories below.");
            return PartialView("_CpvSearchFragment", errorModel);
        }
    }

    [HttpPost("/cpv/selection-fragment")]
    public async Task<IActionResult> UpdateSelectionFragment([FromForm] string[]? selectedCodes = null)
    {
        try
        {
            var codes = selectedCodes?.ToList() ?? new List<string>();
            var allCodes = await cpvCodeService.GetByCodesAsync(codes);

            ViewData["SelectedItems"] = allCodes;
            ViewData["SelectionCount"] = codes.Count;

            var viewModel = new CpvSelectionViewModel
            {
                SelectedCodes = codes
            };

            return PartialView("_CpvSelectionFragment", viewModel);
        }
        catch (Exception)
        {
            var errorModel = new CpvSelectionViewModel().WithError("There is a problem updating your selection. Try again.");
            return PartialView("_CpvSelectionFragment", errorModel);
        }
    }

    [HttpGet("/cpv/children-fragment/{parentCode}")]
    public async Task<IActionResult> GetChildrenFragment(string parentCode, [FromQuery] string[]? selectedCodes = null)
    {
        if (string.IsNullOrWhiteSpace(parentCode))
        {
            return BadRequest("Parent code is required");
        }

        try
        {
            var children = await cpvCodeService.GetChildrenAsync(parentCode);
            var viewModel = new CpvSelectionViewModel
            {
                RootCodes = children,
                SelectedCodes = selectedCodes?.ToList() ?? new List<string>()
            };

            return PartialView("_CpvChildrenFragment", viewModel);
        }
        catch (Exception)
        {
            var errorModel = new CpvSelectionViewModel().WithError("There is a problem loading the subcategories. Try again.");
            return PartialView("_CpvChildrenFragment", errorModel);
        }
    }

    private static CpvCodeDto? FindAndExpandNode(List<CpvCodeDto> codes, string targetCode, List<CpvCodeDto> children)
    {
        foreach (var code in codes)
        {
            if (code.Code == targetCode)
            {
                code.Children = children;
                return code;
            }

            var found = FindAndExpandNode(code.Children, targetCode, children);
            if (found != null)
                return found;
        }
        return null;
    }
}

