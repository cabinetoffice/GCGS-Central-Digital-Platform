using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public sealed class CpvCodeSelection : HierarchicalCodeSelection<CpvCodeDto>
{
    public string BrowseLinkText => HasSelections ? "Edit selected CPV codes" : "Browse CPV codes";

    public IEnumerable<(string Name, string Value)> GetHiddenInputs(string fieldName) =>
        SelectedCodes.Select(code => (fieldName, code));

    protected override CpvCodeDto CreateCodeItem(string code, string descriptionEn, string descriptionCy)
    {
        return new CpvCodeDto
        {
            Code = code,
            DescriptionEn = descriptionEn,
            DescriptionCy = descriptionCy
        };
    }
}