using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class CpvSelectionViewModel
{
    public List<CpvCodeDto> RootCodes { get; set; } = [];
    public List<CpvCodeDto> SearchResults { get; set; } = [];
    public List<string> SelectedCodes { get; set; } = [];
    public string? SearchQuery { get; set; }
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExpandedCode { get; set; }

    public bool IsSelected(string code) => SelectedCodes.Contains(code);

    public int SelectionCount => SelectedCodes.Count;

    public List<CpvCodeDto> GetSelectedItems(List<CpvCodeDto> allCodes)
    {
        return SelectedCodes
            .Select(code => FindCpvCode(allCodes, code))
            .Where(item => item != null)
            .Cast<CpvCodeDto>()
            .ToList();
    }

    private static CpvCodeDto? FindCpvCode(List<CpvCodeDto> codes, string targetCode)
    {
        foreach (var code in codes)
        {
            if (code.Code == targetCode)
                return code;

            var found = FindCpvCode(code.Children, targetCode);
            if (found != null)
                return found;
        }

        return null;
    }

    public CpvSelectionViewModel ToggleSelection(string code)
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

        return new CpvSelectionViewModel
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

    public CpvSelectionViewModel WithSearchResults(List<CpvCodeDto> results, string query)
    {
        return new CpvSelectionViewModel
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

    public CpvSelectionViewModel WithRootCodes(List<CpvCodeDto> roots)
    {
        return new CpvSelectionViewModel
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

    public CpvSelectionViewModel WithError(string error)
    {
        return new CpvSelectionViewModel
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

    public CpvSelectionViewModel WithLoading(bool loading)
    {
        return new CpvSelectionViewModel
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