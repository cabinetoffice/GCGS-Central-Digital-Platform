using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class LocationCodeSelection : HierarchicalCodeSelection<NutsCodeDto>
{
    public override string BrowseLinkText => HasSelections ? "Edit selected locations" : "Browse locations";

    public override IEnumerable<(string Name, string Value)> GetHiddenInputs(string fieldName = "location") =>
        SelectedCodes.Select(code => (fieldName, code));


    protected override NutsCodeDto CreateCodeItem(string code, string descriptionEn, string descriptionCy)
    {
        return new NutsCodeDto
        {
            Code = code,
            DescriptionEn = descriptionEn,
            DescriptionCy = descriptionCy
        };
    }
}