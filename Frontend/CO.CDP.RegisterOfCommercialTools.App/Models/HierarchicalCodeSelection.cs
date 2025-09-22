using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class HierarchicalCodeSelection<T> : IHierarchicalCodeSelection where T : IHierarchicalCodeDto
{
    public List<string> SelectedCodes { get; set; } = [];

    public List<T> SelectedItems { get; set; } = [];

    public string? SearchQuery { get; set; }

    public List<string> ExpandedNodes { get; set; } = [];

    public bool HasSelections => SelectedCodes.Count > 0;

    public string SelectionSummary => $"Selected ({SelectedCodes.Count})";

    public virtual string BrowseLinkText => HasSelections ? "Edit selection" : "Browse codes";

    public virtual IEnumerable<(string Name, string Value)> GetHiddenInputs(string fieldName = "codes") =>
        SelectedCodes.Select(code => (fieldName, code));

    public void AddSelection(string code, string descriptionEn, string descriptionCy)
    {
        if (!SelectedCodes.Contains(code))
        {
            SelectedCodes.Add(code);

            var item = CreateCodeItem(code, descriptionEn, descriptionCy);
            if (item != null)
            {
                SelectedItems.Add(item);
            }
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

    IEnumerable<IHierarchicalCodeDto> IHierarchicalCodeSelection.SelectedItems =>
        SelectedItems.Cast<IHierarchicalCodeDto>();

    protected virtual T? CreateCodeItem(string code, string descriptionEn, string descriptionCy)
    {
        return default(T);
    }
}