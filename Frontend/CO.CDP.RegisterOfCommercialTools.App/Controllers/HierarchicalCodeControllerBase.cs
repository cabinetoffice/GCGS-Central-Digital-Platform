using Microsoft.AspNetCore.Mvc;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Controllers;

public abstract class HierarchicalCodeControllerBase<T>(IHierarchicalCodeService<T> codeService) : Controller
    where T : IHierarchicalCodeDto
{
    protected abstract string CodeTypeName { get; }
    protected abstract string RoutePrefix { get; }
    protected abstract string TreeFragmentView { get; }
    protected abstract string SearchFragmentView { get; }
    protected abstract string SelectionFragmentView { get; }
    protected abstract string ChildrenFragmentView { get; }

    [HttpGet]
    public virtual async Task<IActionResult> GetTreeFragment([FromQuery] string[]? selectedCodes = null, [FromQuery] string? expandedCode = null)
    {
        try
        {
            var rootCodes = await codeService.GetRootCodesAsync();
            var viewModel = new HierarchicalCodeSelectionViewModel<T>
            {
                RootCodes = rootCodes,
                SelectedCodes = selectedCodes?.ToList() ?? new List<string>(),
                ExpandedCode = expandedCode
            };

            if (!string.IsNullOrEmpty(expandedCode))
            {
                var children = await codeService.GetChildrenAsync(expandedCode);
                var expandedNode = FindAndExpandNode(rootCodes, expandedCode, children);
                if (expandedNode != null)
                {
                    viewModel = viewModel.WithRootCodes(rootCodes);
                }
            }

            return PartialView(TreeFragmentView, viewModel);
        }
        catch (Exception)
        {
            var errorModel = new HierarchicalCodeSelectionViewModel<T>().WithError($"There is a problem loading {CodeTypeName} codes. Try refreshing the page.");
            return PartialView(TreeFragmentView, errorModel);
        }
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetSearchFragment([FromQuery] string q, [FromQuery] string[]? selectedCodes = null)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            var emptyModel = new HierarchicalCodeSelectionViewModel<T>
            {
                SelectedCodes = selectedCodes?.ToList() ?? new List<string>(),
                SearchQuery = q
            };
            return PartialView(SearchFragmentView, emptyModel);
        }

        try
        {
            var searchResults = await codeService.SearchAsync(q);
            var viewModel = new HierarchicalCodeSelectionViewModel<T>
            {
                SearchResults = searchResults,
                SelectedCodes = selectedCodes?.ToList() ?? new List<string>(),
                SearchQuery = q
            };

            return PartialView(SearchFragmentView, viewModel);
        }
        catch (Exception)
        {
            var errorModel = new HierarchicalCodeSelectionViewModel<T>().WithError("There is a problem with search. Try again or browse the categories below.");
            return PartialView(SearchFragmentView, errorModel);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> UpdateSelectionFragment([FromForm] string[]? selectedCodes = null)
    {
        try
        {
            var codes = selectedCodes?.ToList() ?? new List<string>();
            var allCodes = await codeService.GetByCodesAsync(codes);

            ViewData["SelectedItems"] = allCodes;
            ViewData["SelectionCount"] = codes.Count;

            var viewModel = new HierarchicalCodeSelectionViewModel<T>
            {
                SelectedCodes = codes
            };

            return PartialView(SelectionFragmentView, viewModel);
        }
        catch (Exception)
        {
            var errorModel = new HierarchicalCodeSelectionViewModel<T>().WithError("There is a problem updating your selection. Try again.");
            return PartialView(SelectionFragmentView, errorModel);
        }
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetChildrenFragment(string parentCode, [FromQuery] string[]? selectedCodes = null)
    {
        if (string.IsNullOrWhiteSpace(parentCode))
        {
            return BadRequest("Parent code is required");
        }

        try
        {
            var children = await codeService.GetChildrenAsync(parentCode);
            var viewModel = new HierarchicalCodeSelectionViewModel<T>
            {
                RootCodes = children,
                SelectedCodes = selectedCodes?.ToList() ?? new List<string>()
            };

            return PartialView(ChildrenFragmentView, viewModel);
        }
        catch (Exception)
        {
            var errorModel = new HierarchicalCodeSelectionViewModel<T>().WithError("There is a problem loading the subcategories. Try again.");
            return PartialView(ChildrenFragmentView, errorModel);
        }
    }

    private static T? FindAndExpandNode(List<T> codes, string targetCode, List<T> children)
    {
        foreach (var code in codes)
        {
            if (code.Code == targetCode)
            {
                SetChildren(code, children);
                return code;
            }

            var found = FindAndExpandNode(GetChildren(code), targetCode, children);
            if (found != null)
                return found;
        }
        return default(T);
    }

    private static List<T> GetChildren(T code)
    {
        if (code is CpvCodeDto cpvCode)
        {
            return cpvCode.Children.Cast<T>().ToList();
        }

        return [];
    }

    private static void SetChildren(T code, List<T> children)
    {
        if (code is CpvCodeDto cpvCode && children is List<CpvCodeDto> cpvChildren)
        {
            cpvCode.Children = cpvChildren;
        }
    }
}