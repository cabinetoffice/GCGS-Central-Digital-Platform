using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class HierarchicalCodeSelectionViewModel<T> where T : IHierarchicalCodeDto
{
    public List<T> RootCodes { get; set; } = [];
    public List<T> SearchResults { get; set; } = [];
    public List<string> SelectedCodes { get; set; } = [];
    public string? SearchQuery { get; set; }
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExpandedCode { get; set; }

    public bool IsSelected(string code) => SelectedCodes.Contains(code);

    public int SelectionCount => SelectedCodes.Count;

    public List<T> GetSelectedItems(List<T> allCodes)
    {
        return SelectedCodes
            .Select(code => FindCode(allCodes, code))
            .Where(item => item != null)
            .Cast<T>()
            .ToList();
    }

    private static T? FindCode(List<T> codes, string targetCode)
    {
        foreach (var code in codes)
        {
            if (code.Code == targetCode)
                return code;

            var found = FindCode(GetChildren(code), targetCode);
            if (found != null)
                return found;
        }

        return default(T);
    }

    private static List<T> GetChildren(T code)
    {
        if (code is CpvCodeDto cpvCode)
        {
            return cpvCode.Children.Cast<T>().ToList();
        }

        if (code is NutsCodeDto nutsCode)
        {
            return nutsCode.Children.Cast<T>().ToList();
        }

        return [];
    }

    public HierarchicalCodeSelectionViewModel<T> ToggleSelection(string code)
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

        return new HierarchicalCodeSelectionViewModel<T>
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

    public HierarchicalCodeSelectionViewModel<T> WithSearchResults(List<T> results, string query)
    {
        return new HierarchicalCodeSelectionViewModel<T>
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

    public HierarchicalCodeSelectionViewModel<T> WithRootCodes(List<T> roots)
    {
        return new HierarchicalCodeSelectionViewModel<T>
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

    public HierarchicalCodeSelectionViewModel<T> WithError(string error)
    {
        return new HierarchicalCodeSelectionViewModel<T>
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

    public HierarchicalCodeSelectionViewModel<T> WithLoading(bool loading)
    {
        return new HierarchicalCodeSelectionViewModel<T>
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