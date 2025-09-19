using Microsoft.AspNetCore.Mvc;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Controllers;

public class CpvCodesController(IHierarchicalCodeService<CpvCodeDto> codeService) : HierarchicalCodeControllerBase<CpvCodeDto>(codeService)
{
    protected override string CodeTypeName => "CPV";
    protected override string RoutePrefix => "cpv";
    protected override string TreeFragmentView => "_CpvTreeFragment";
    protected override string SearchFragmentView => "_CpvSearchFragment";
    protected override string SelectionFragmentView => "_CpvSelectionFragment";
    protected override string ChildrenFragmentView => "_CpvChildrenFragment";

    [HttpGet("/cpv/tree-fragment")]
    public override async Task<IActionResult> GetTreeFragment([FromQuery] string[]? selectedCodes = null, [FromQuery] string? expandedCode = null)
    {
        return await base.GetTreeFragment(selectedCodes, expandedCode);
    }

    [HttpGet("/cpv/search-fragment")]
    public override async Task<IActionResult> GetSearchFragment([FromQuery] string q, [FromQuery] string[]? selectedCodes = null)
    {
        return await base.GetSearchFragment(q, selectedCodes);
    }

    [HttpPost("/cpv/selection-fragment")]
    public override async Task<IActionResult> UpdateSelectionFragment([FromForm] string[]? selectedCodes = null)
    {
        return await base.UpdateSelectionFragment(selectedCodes);
    }

    [HttpGet("/cpv/children-fragment/{parentCode}")]
    public override async Task<IActionResult> GetChildrenFragment(string parentCode, [FromQuery] string[]? selectedCodes = null)
    {
        return await base.GetChildrenFragment(parentCode, selectedCodes);
    }
}

