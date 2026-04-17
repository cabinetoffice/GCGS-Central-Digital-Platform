using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.Pages.Users;

/// <summary>
/// Page object for the join request confirmation page in the UserManagement.App.
/// Covers both Approve and Reject confirmation pages (same view, different action).
/// URL pattern: /organisation/{slug}/join-requests/{id}/approve?personId={personId}
/// </summary>
public class JoinRequestConfirmPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    private const string HeadingSelector = "h1.govuk-heading-l";
    private const string ConfirmButtonSelector = "button[type='submit'].govuk-button";
    private const string SummaryListSelector = ".govuk-summary-list";

    public JoinRequestConfirmPage(IPage page)
    {
        _page = page;
        _baseUrl = ConfigUtility.GetUserManagementAppBaseUrl();
    }

    public async Task NavigateToApprove(string organisationSlug, Guid joinRequestId, Guid personId)
    {
        var url = $"{_baseUrl}/organisation/{organisationSlug}/join-requests/{joinRequestId}/approve?personId={personId}";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to join request approve page: {url}");
    }

    public async Task NavigateToReject(string organisationSlug, Guid joinRequestId, Guid personId)
    {
        var url = $"{_baseUrl}/organisation/{organisationSlug}/join-requests/{joinRequestId}/reject?personId={personId}";
        await _page.GotoAsync(url);
        Console.WriteLine($"📌 Navigated to join request reject page: {url}");
    }

    public async Task<string> GetPageHeading()
    {
        await _page.WaitForSelectorAsync(HeadingSelector);
        return await _page.InnerTextAsync(HeadingSelector);
    }

    public async Task<string> GetCurrentUrl() => _page.Url;

    public async Task ClickConfirm() =>
        await _page.ClickAsync(ConfirmButtonSelector);

    public async Task<bool> HasSummaryList() =>
        await _page.IsVisibleAsync(SummaryListSelector);
}
