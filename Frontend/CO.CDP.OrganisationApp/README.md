# CO.CO.CDP.OrganisationInformation.OrganisationApp

## TagHelpers

### Pagination

The `Pagination` partial view is used to display pagination controls for lists of items in the application. It provides navigation links for moving between pages and handles pagination logic internally.

#### How to Use

To include the pagination partial view in your Razor page, use the `Html.PartialAsync` method to pass the required parameters.

#### Example Usage

```razor
@await Html.PartialAsync("_Pagination", new PaginationPartialModel
{
    CurrentPage = Model.CurrentPage,
    TotalItems = Model.TotalOrganisations,
    PageSize = Model.PageSize,
    Url = "/support/organisations"
})
```

#### Parameters

The `PaginationPartialModel` requires the following parameters:

- `CurrentPage` *(int)*: The current page number.
- `TotalItems` *(int)*: The total number of items available.
- `PageSize` *(int)*: The number of items displayed per page.
- `Url` *(string)*: The base URL to be used for pagination links.

#### Pagination Logic

The `PaginationPartialModel` calculates the total number of pages automatically based on the `TotalItems` and `PageSize` values. It generates page URLs dynamically using the provided `Url` parameter.

#### Example Output

```
< Previous  1  2  3  ...  10  Next >
```

#### File Locations

Ensure the following files are present in the project:

- **Partial View:** `Pages/Shared/_Pagination.cshtml`
- **Model:** `Pages/Shared/PaginationPartialModel.cs`

#### Styling

The pagination component follows GOV.UK design standards and is styled using the `govuk-pagination` class to ensure consistency across the application.