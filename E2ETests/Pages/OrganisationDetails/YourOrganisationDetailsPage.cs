using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages;

public class YourOrganisationDetailsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // ✅ Page Locators
    private readonly string PageTitleSelector = "h1.govuk-heading-l";
    private readonly string SupplierTypeChangeLink = "a[href*='individual-or-org']";
    private readonly string RegisteredAddressChangeLink = "a[href*='registered-address/uk']";
    private readonly string PostalAddressAddLink = "a[href*='postal-address']";
    private readonly string VatNumberAddLink = "a[href*='vat-question']";
    private readonly string WebsiteChangeLink = "a[href*='website-question']";
    private readonly string EmailChangeLink = "a[href*='email-address']";
    private readonly string OrganisationTypeAddLink = "a[href*='operation']";
    private readonly string LegalFormAddLink = "a[href*='company-act-question']";

    public YourOrganisationDetailsPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetBaseUrl();
    }


    public async Task NavigateTo(string organisationKey)
    {
        var organisationId = StorageUtility.Retrieve(organisationKey);
        if (string.IsNullOrEmpty(organisationId))
            throw new System.Exception($"❌ Organisation ID not found for key: {organisationKey}");

        var url = $"{_baseUrl}/organisation/{organisationId}/supplier-information";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to Your Organisation Details Page: {url}");

        await _page.WaitForSelectorAsync(PageTitleSelector);
    }

    public async Task ClickChangeWebsiteAddress()
    {
        await _page.ClickAsync("a[href*='website-question']");
        Console.WriteLine("✅ Clicked 'Change Website Address'");
    }

    public async Task ClickChangeEmailAddress()
    {
        await _page.ClickAsync("a[href*='email-address']");
        Console.WriteLine("✅ Clicked 'Change Email Address'");
    }

    public async Task<string> GetPageTitle()
    {
        return await _page.InnerTextAsync(PageTitleSelector);
    }

    public async Task<string> AssertOrganisationAndPostalAddressSame()
    {
        // Selectors for the summary list values — these assume the postal address is always after the registered address
        var registeredAddressSelector = "div.govuk-summary-list__row:nth-of-type(2) dd.govuk-summary-list__value";
        var postalAddressSelector = "div.govuk-summary-list__row:nth-of-type(3) dd.govuk-summary-list__value";

        var registeredAddress = (await _page.InnerTextAsync(registeredAddressSelector)).Trim();
        var postalAddress = (await _page.InnerTextAsync(postalAddressSelector)).Trim();

        if (string.IsNullOrWhiteSpace(postalAddress))
            throw new System.Exception("❌ Postal address is empty — it should be the same as registered address");

        if (registeredAddress != postalAddress)
            throw new System.Exception($"❌ Postal address does not match registered address:\nRegistered: {registeredAddress}\nPostal: {postalAddress}");

        Console.WriteLine("✅ Postal address matches registered address");
        return postalAddress;
    }


    // ✅ Click actions
    public async Task ClickChangeSupplierType() => await _page.ClickAsync(SupplierTypeChangeLink);
    public async Task ClickChangeRegisteredAddress() => await _page.ClickAsync(RegisteredAddressChangeLink);
    public async Task ClickAddPostalAddress() => await _page.ClickAsync(PostalAddressAddLink);
    public async Task ClickAddVatNumber() => await _page.ClickAsync(VatNumberAddLink);
    public async Task ClickChangeWebsite() => await _page.ClickAsync(WebsiteChangeLink);
    public async Task ClickChangeEmail() => await _page.ClickAsync(EmailChangeLink);
    public async Task ClickAddOrganisationType() => await _page.ClickAsync(OrganisationTypeAddLink);
    public async Task ClickAddLegalForm() => await _page.ClickAsync(LegalFormAddLink);

    public async Task<string> GetRegisteredAddress()
    {
        var addressSelector = "dt:has-text(\"Registered address\") + dd.govuk-summary-list__value";
        await _page.WaitForSelectorAsync(addressSelector);
        var addressText = await _page.InnerTextAsync(addressSelector);
        Console.WriteLine($"📌 Registered Address: {addressText}");
        return addressText.Trim();
    }

    public async Task<string> GetPostalAddress()
    {
        var addressSelector = "dt:has-text(\"Postal address\") + dd.govuk-summary-list__value";
        await _page.WaitForSelectorAsync(addressSelector);
        var addressText = await _page.InnerTextAsync(addressSelector);
        Console.WriteLine($"📌 Postal Address: {addressText}");
        return addressText.Trim();
    }

    public async Task AssertRegisteredAddressContains(string expectedAddressPart)
    {
        var address = await GetRegisteredAddress();
        if (!address.Contains(expectedAddressPart, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception($"❌ Expected registered address to contain '{expectedAddressPart}', but got '{address}'");
        }
        Console.WriteLine($"✅ Registered address contains expected value: {expectedAddressPart}");
    }

    public async Task AssertPostalAddressContains(string expectedAddressPart)
    {
        var address = await GetPostalAddress();
        if (!address.Contains(expectedAddressPart, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception($"❌ Expected postal address to contain '{expectedAddressPart}', but got '{address}'");
        }
        Console.WriteLine($"✅ Postal address contains expected value: {expectedAddressPart}");
    }


    // Example composite usage
    public async Task AssertLoaded()
    {
        var title = await GetPageTitle();
        if (title.Trim() != "Your organisation details")
            throw new System.Exception("❌ Unexpected page title: " + title);
    }

    public async Task AssertRegisteredAddressDoesNotExist()
    {
        var content = await _page.ContentAsync();
        if (content.Contains("Registered address"))
            throw new Exception("❌ Registered address should not be present for Individual supplier type.");
        Console.WriteLine("✅ No Registered address found (expected)");
    }

    public async Task AssertSupplierType(string expectedType)
    {
        var actual = await _page.InnerTextAsync("dl.govuk-summary-list > div:nth-child(1) dd.govuk-summary-list__value");

        if (!actual.Trim().Equals(expectedType, StringComparison.OrdinalIgnoreCase))
            throw new Exception($"❌ Expected supplier type '{expectedType}' but found '{actual}'");

        Console.WriteLine($"✅ Supplier type is '{expectedType}'");
    }
}
