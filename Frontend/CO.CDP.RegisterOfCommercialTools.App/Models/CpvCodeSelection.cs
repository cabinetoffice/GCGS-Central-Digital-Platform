using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class CpvCodeSelection
{
    public List<string> SelectedCodes { get; set; } = [];

    public List<CpvCodeDto> SelectedItems { get; set; } = [];

    public string? SearchQuery { get; set; }

    public List<string> ExpandedNodes { get; set; } = [];

    public bool HasSelections => SelectedCodes.Count > 0;

    public string SelectionSummary => $"Selected ({SelectedCodes.Count})";

    public string BrowseLinkText => HasSelections ? "Edit CPV code selection" : "Browse CPV codes";

    public IEnumerable<(string Name, string Value)> GetHiddenInputs() =>
        SelectedCodes.Select(code => ("cpv", code));

    public void AddSelection(string code, string descriptionEn, string descriptionCy)
    {
        if (!SelectedCodes.Contains(code))
        {
            SelectedCodes.Add(code);
            SelectedItems.Add(new CpvCodeDto {
                Code = code,
                DescriptionEn = descriptionEn,
                DescriptionCy = descriptionCy
            });
        }
    }

    public void RemoveSelection(string code)
    {
        SelectedCodes.Remove(code);
        SelectedItems.RemoveAll(x => x.Code == code);
    }

    public bool IsSelected(string code) => SelectedCodes.Contains(code);

    public void Clear()
    {
        SelectedCodes.Clear();
        SelectedItems.Clear();
    }
}