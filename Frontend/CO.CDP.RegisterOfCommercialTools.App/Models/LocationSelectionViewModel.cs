using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class LocationSelectionViewModel : HierarchicalCodeSelectionViewModel<NutsCodeDto>
{
    private static List<NutsCodeDto> GetChildren(NutsCodeDto code)
    {
        return code.Children;
    }

    private static void SetChildren(NutsCodeDto code, List<NutsCodeDto> children)
    {
        code.Children = children;
    }

    public new LocationSelectionViewModel ToggleSelection(string code)
    {
        var newSelectedCodes = new List<string>(SelectedCodes);

        if (newSelectedCodes.Contains(code))
        {
            newSelectedCodes.Remove(code);
        }
        else
        {
            newSelectedCodes.Add(code);
        }

        return new LocationSelectionViewModel
        {
            RootCodes = RootCodes,
            SearchResults = SearchResults,
            SelectedCodes = newSelectedCodes,
            SearchQuery = SearchQuery,
            IsLoading = IsLoading,
            ErrorMessage = ErrorMessage,
            ExpandedCode = ExpandedCode
        };
    }

    public new LocationSelectionViewModel WithSearchResults(List<NutsCodeDto> results, string query)
    {
        return new LocationSelectionViewModel
        {
            RootCodes = RootCodes,
            SearchResults = results,
            SelectedCodes = SelectedCodes,
            SearchQuery = query,
            IsLoading = false,
            ErrorMessage = null,
            ExpandedCode = ExpandedCode
        };
    }

    public new LocationSelectionViewModel WithRootCodes(List<NutsCodeDto> roots)
    {
        return new LocationSelectionViewModel
        {
            RootCodes = roots,
            SearchResults = SearchResults,
            SelectedCodes = SelectedCodes,
            SearchQuery = SearchQuery,
            IsLoading = false,
            ErrorMessage = null,
            ExpandedCode = ExpandedCode
        };
    }

    public new LocationSelectionViewModel WithError(string error)
    {
        return new LocationSelectionViewModel
        {
            RootCodes = RootCodes,
            SearchResults = SearchResults,
            SelectedCodes = SelectedCodes,
            SearchQuery = SearchQuery,
            IsLoading = false,
            ErrorMessage = error,
            ExpandedCode = ExpandedCode
        };
    }

    public new LocationSelectionViewModel WithLoading(bool loading)
    {
        return new LocationSelectionViewModel
        {
            RootCodes = RootCodes,
            SearchResults = SearchResults,
            SelectedCodes = SelectedCodes,
            SearchQuery = SearchQuery,
            IsLoading = loading,
            ErrorMessage = null,
            ExpandedCode = ExpandedCode
        };
    }
}