using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class CpvCodeSelection : HierarchicalCodeSelection<CpvCodeDto>
{
    public override string BrowseLinkText => HasSelections ? "Edit CPV code selection" : "Browse CPV codes";

    public override IEnumerable<(string Name, string Value)> GetHiddenInputs(string fieldName = "cpv") =>
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