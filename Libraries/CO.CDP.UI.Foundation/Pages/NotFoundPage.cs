namespace CO.CDP.UI.Foundation.Pages;

public class NotFoundPage
{
    public string Render(string title = "Page not found")
    {
        return $@"
            <div class=""govuk-grid-row"">
                <div class=""govuk-grid-column-two-thirds"">
                    <h1 class=""govuk-heading-l"">{title}</h1>
                    <p class=""govuk-body"">
                        If you typed the web address, check it is correct.
                    </p>
                    <p class=""govuk-body"">
                        If you pasted the web address, check you copied the entire address.
                    </p>
                </div>
            </div>";
    }
}