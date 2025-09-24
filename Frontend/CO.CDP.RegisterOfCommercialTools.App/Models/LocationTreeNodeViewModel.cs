using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class LocationTreeNodeViewModel
{
    public required NutsCodeDto Item { get; set; }
    public int Level { get; set; }
    public List<string> SelectedCodes { get; set; } = [];

    public bool IsSelected => SelectedCodes.Contains(Item.Code);
    public bool HasChildren => Item.HasChildren;
    public string CheckboxId => $"location-checkbox-{Item.Code.Replace("\\D", "")}";

    public bool IsDisabled => !Item.IsSelectable || HasAncestorSelected();

    private bool HasAncestorSelected()
    {
        return Item.ParentCode != null && SelectedCodes.Any(selectedCode => IsAncestorOf(selectedCode, Item.Code));
    }

    private static bool IsAncestorOf(string potentialAncestor, string code)
    {
        return potentialAncestor.Length < code.Length && code.StartsWith(potentialAncestor);
    }
}