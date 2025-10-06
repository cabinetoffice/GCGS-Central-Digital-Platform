using Microsoft.AspNetCore.Mvc;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Controllers;

public class LocationCodesController(IHierarchicalCodeService<NutsCodeDto> codeService, ILogger<LocationCodesController> logger) : HierarchicalCodeControllerBase<NutsCodeDto>(codeService)
{
    private readonly IHierarchicalCodeService<NutsCodeDto> _locationCodeService = codeService;

    protected override string CodeTypeName => "Location";
    protected override string RoutePrefix => "location";
    protected override string TreeFragmentView => "_LocationTreeFragment";
    protected override string SearchFragmentView => "_LocationSearchFragment";
    protected override string SelectionFragmentView => "_LocationSelectionFragment";
    protected override string ChildrenFragmentView => "_LocationChildrenFragment";

    [HttpGet("/location/tree-fragment")]
    public override async Task<IActionResult> GetTreeFragment([FromQuery] string[]? selectedCodes = null, [FromQuery] string? expandedCode = null)
    {
        logger.LogInformation("Location tree fragment requested: SelectedCodes={SelectedCodes}, ExpandedCode={ExpandedCode}",
            selectedCodes?.Length ?? 0, (expandedCode ?? "none").Replace("\r", "").Replace("\n", ""));

        return await base.GetTreeFragment(selectedCodes, expandedCode);
    }

    [HttpGet("/location/search-fragment")]
    public override async Task<IActionResult> GetSearchFragment([FromQuery] string? q, [FromQuery] string[]? selectedCodes = null)
    {
        logger.LogInformation("Location search requested: Query='{Query}', SelectedCodes={SelectedCodeCount}",
            (q ?? string.Empty).Replace("\r", "").Replace("\n", ""), selectedCodes?.Length ?? 0);

        return await base.GetSearchFragment(q ?? string.Empty, selectedCodes);
    }

    [HttpPost("/location/selection-fragment")]
    public override async Task<IActionResult> UpdateSelectionFragment([FromForm] string[]? selectedCodes = null)
    {
        logger.LogInformation("Location selection updated: SelectedCodes=[{SelectedCodes}]",
            string.Join(", ", (selectedCodes ?? []).Select(code => code.Replace("\r", "").Replace("\n", ""))));

        return await base.UpdateSelectionFragment(selectedCodes);
    }

    [HttpGet("/location/children-fragment/{parentCode}")]
    public override async Task<IActionResult> GetChildrenFragment(string parentCode, [FromQuery] string[]? selectedCodes = null)
    {
        logger.LogInformation("Location children requested: ParentCode={ParentCode}, SelectedCodes={SelectedCodeCount}",
            parentCode.Replace("\r", "").Replace("\n", ""), selectedCodes?.Length ?? 0);

        return await base.GetChildrenFragment(parentCode, selectedCodes);
    }

    [HttpPost("/location/accordion-selection-fragment")]
    public async Task<IActionResult> GetAccordionSelectionFragment([FromForm] string[]? selectedCodes = null)
    {
        logger.LogInformation("Location accordion selection requested: SelectedCodes=[{SelectedCodes}]",
            string.Join(", ", (selectedCodes ?? []).Select(code => code.Replace("\r", "").Replace("\n", ""))));

        if (selectedCodes == null || selectedCodes.Length == 0)
        {
            return PartialView("_LocationAccordionSelectionFragment", new List<NutsCodeDto>());
        }

        var locations = await _locationCodeService.GetByCodesAsync(selectedCodes.ToList());
        return PartialView("_LocationAccordionSelectionFragment", locations);
    }
}