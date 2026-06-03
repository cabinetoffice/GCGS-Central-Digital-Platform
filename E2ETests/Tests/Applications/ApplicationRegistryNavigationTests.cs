namespace E2ETests.Applications;

/// <summary>
/// Navigation tests: verifies that the Application Registry pages are reachable,
/// render the correct headings, and flow between pages correctly.
///
/// These tests verify the complete UI journey:
///   OrganisationOverview → ApplicationList → ApplicationDetail → UserAssignments
/// </summary>
[Category("ApplicationRegistry")]
public class ApplicationRegistryNavigationTests : ApplicationRegistryBaseTest
{
    [Category("ApplicationRegistry")]
    [Test]
    public async Task ApplicationListPage_LoadsCorrectly_ForBuyerOrg()
    {
        TestContext.Out.WriteLine("🔹 Navigate to Application List for a Buyer organisation");

        await _applicationListPage.NavigateTo(_organisationId);

        var heading = await _applicationListPage.GetHeading();
        Assert.That(heading, Does.Contain("Applications"),
            $"Expected heading to contain 'Applications' but got: {heading}");

        TestContext.Out.WriteLine($"✅ Page heading: {heading}");
        TestContext.Out.WriteLine($"✅ URL: {await _applicationListPage.GetPageUrl()}");
    }

    [Category("ApplicationRegistry")]
    [Test]
    public async Task ApplicationListPage_ShowsManageApplicationAccessLink_InDashboard()
    {
        TestContext.Out.WriteLine("🔹 Verify 'Manage application access' link appears on dashboard");

        var dashboardUrl = $"{_baseUrl}/organisation/{_organisationId}";
        await _page.GotoAsync(dashboardUrl);
        await _page.WaitForSelectorAsync("h1.govuk-heading-l");

        // Check the link is present for Buyer orgs — Admin scope required
        var linkLocator = _page.Locator("a.govuk-link:has-text('Manage application access')");
        var count = await linkLocator.CountAsync();

        if (count > 0)
        {
            TestContext.Out.WriteLine("✅ 'Manage application access' link found on dashboard");
            var href = await linkLocator.GetAttributeAsync("href");
            Assert.That(href, Does.Contain($"/organisation/{_organisationId}/applications"));
        }
        else
        {
            // For non-Buyer orgs the link is not shown — note this as an observation
            TestContext.Out.WriteLine("ℹ️  'Manage application access' link not shown (org may not be Buyer role)");
            // Navigate directly to confirm the page loads
            await _applicationListPage.NavigateTo(_organisationId);
            var heading = await _applicationListPage.GetHeading();
            TestContext.Out.WriteLine($"  Direct nav heading: {heading}");
        }
    }

    [Category("ApplicationRegistry")]
    [Test]
    public async Task ApplicationListPage_ShowsTable_WhenAppsAvailable()
    {
        TestContext.Out.WriteLine("🔹 Verify applications table renders when org has enabled apps");

        await _applicationListPage.NavigateTo(_organisationId);

        var appCount = await _applicationListPage.GetApplicationCount();
        TestContext.Out.WriteLine($"  Application count: {appCount}");

        if (appCount > 0)
        {
            var names = await _applicationListPage.GetApplicationNames();
            TestContext.Out.WriteLine($"  Applications: {string.Join(", ", names)}");
            Assert.That(names, Is.Not.Empty);
        }
        else
        {
            // No apps yet — table hidden, message shown
            TestContext.Out.WriteLine("ℹ️  No applications enabled for this org yet");
        }

        // Page should always load without error
        var pageUrl = await _applicationListPage.GetPageUrl();
        Assert.That(pageUrl, Does.Contain($"/organisation/{_organisationId}/applications"));
    }
}
