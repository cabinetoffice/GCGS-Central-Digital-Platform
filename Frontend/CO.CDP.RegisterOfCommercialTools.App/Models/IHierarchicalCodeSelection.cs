using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public interface IHierarchicalCodeSelection
{
    List<string> SelectedCodes { get; }
    bool HasSelections { get; }
    IEnumerable<IHierarchicalCodeDto> SelectedItems { get; }
}