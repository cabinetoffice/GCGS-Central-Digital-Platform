using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class OrganisationSelectionsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    private readonly string OrganisationNameSelector = "//dt[text()='Organisation name']/following-sibling::dd/p";
    private readonly string OrganisationIdentifierSelector = "//dt[text()='Organisation identifier']/following-sibling::dd/ul/li[2]";
    private readonly string OrganisationEmailSelector = "//dt[text()='Organisation email']/following-sibling::dd/p";

    private readonly string OrganisationChangeLink = "//dt[text()='Organisation name']/following-sibling::dd[@class='govuk-summary-list__actions']/a";
    private readonly string OrganisationIdentifierAddLink = "//dt[text()='Organisation identifier']/following-sibling::dd[@class='govuk-summary-list__actions']/a";
    private readonly string OrganisationEmailChangeLink = "//dt[text()='Organisation email']/following-sibling::dd[@class='govuk-summary-list__actions']/a";

    private readonly string CompleteSupplierInfoLink = "a[href*='/supplier-information']";
    private readonly string ManageUsersLink = "a[href*='/users/user-summary']";
    private readonly string RegisterAsBuyerLink = "a[href*='/register-supplier-as-buyer']";

    public OrganisationSelectionsPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl(); // Get base URL from configuration utility
    }

    public async Task NavigateTo(string storageKey)
    {
        string organisationId = StorageUtility.Retrieve(storageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new System.Exception($"❌ Organisation ID not found for key: {storageKey}");
        }

        string url = $"{_baseUrl}/organisation/{organisationId}";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Organisation Details: {url}");

        await _page.WaitForSelectorAsync(OrganisationNameSelector);
    }

    public async Task<string> GetOrganisationName()
    {
        await _page.WaitForSelectorAsync(OrganisationNameSelector);
        return await _page.InnerTextAsync(OrganisationNameSelector);
    }

    public async Task<string> GetOrganisationIdentifier()
    {
        await _page.WaitForSelectorAsync(OrganisationIdentifierSelector);
        return await _page.InnerTextAsync(OrganisationIdentifierSelector);
    }

    public async Task<string> GetOrganisationEmail()
    {
        await _page.WaitForSelectorAsync(OrganisationEmailSelector);
        return await _page.InnerTextAsync(OrganisationEmailSelector);
    }

    public async Task ClickChangeOrganisationName()
    {
        await _page.ClickAsync(OrganisationChangeLink);
    }

    public async Task ClickChangeOrganisationEmail()
    {
        await _page.ClickAsync(OrganisationEmailChangeLink);
    }

    public async Task ClickAddOrganisationIdentifier()
    {
        await _page.ClickAsync(OrganisationIdentifierAddLink);
    }

    public async Task ClickCompleteSupplierInfo()
    {
        await _page.ClickAsync(CompleteSupplierInfoLink);
    }

    public async Task ClickManageUsers()
    {
        await _page.ClickAsync(ManageUsersLink);
    }

    public async Task ClickRegisterAsBuyer()
    {
        await _page.ClickAsync(RegisterAsBuyerLink);
    }
}
