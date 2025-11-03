using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public sealed class LocationCodeSelection : HierarchicalCodeSelection<NutsCodeDto>
{
    public string BrowseLinkText => HasSelections ? "Edit" : "Browse locations";

    public IEnumerable<(string Name, string Value)> GetHiddenInputs(string fieldName) =>
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