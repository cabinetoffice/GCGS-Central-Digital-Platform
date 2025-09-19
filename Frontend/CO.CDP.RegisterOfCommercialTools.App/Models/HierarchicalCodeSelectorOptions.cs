namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class HierarchicalCodeSelectorOptions
{
    public required string CodeType { get; set; }
    public required string ModalTitle { get; set; }
    public required string SearchPlaceholder { get; set; }
    public required string RoutePrefix { get; set; }
    public required string FieldName { get; set; }
    public string BrowseButtonId { get; set; } = "browse-codes";
    public string ModalId { get; set; } = "code-selector-modal";
    public string SearchInputId { get; set; } = "code-search";
    public string TreeContainerId { get; set; } = "code-tree";
    public string SearchContainerId { get; set; } = "search-results-list";
    public string SelectionContainerId { get; set; } = "selected-codes-list";
}