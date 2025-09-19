using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class CpvTreeNodeViewModel
{
    public required CpvCodeDto Item { get; set; }
    public int Level { get; set; }
    public List<string> SelectedCodes { get; set; } = [];

    public bool IsSelected => SelectedCodes.Contains(Item.Code);
    public bool HasChildren => Item.HasChildren;
    public string CheckboxId => $"cpv-checkbox-{Item.Code.Replace("\\D", "")}";
}