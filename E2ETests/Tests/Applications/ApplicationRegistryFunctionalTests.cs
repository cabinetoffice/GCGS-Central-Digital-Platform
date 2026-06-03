namespace E2ETests.Applications;

/// <summary>
/// Functional tests for the Application Registry UI flows:
///  - Application detail page (tabs, roles, permissions)
///  - Assign user journey (pick user → pick roles → check answers → confirm)
///  - Edit roles journey
///  - Revoke access journey
///
/// NOTE: These tests require at least one application enabled for the test org
/// and at least one org member available for assignment.
/// </summary>
[Category("ApplicationRegistry")]
public class ApplicationRegistryFunctionalTests : ApplicationRegistryBaseTest
{
    private string _applicationId = string.Empty;
    private string _applicationName = string.Empty;

    [SetUp]
    public async Task SetupApplicationData()
    {
        // Navigate to applications list and capture first available application
        await _applicationListPage.NavigateTo(_organisationId);
        var appCount = await _applicationListPage.GetApplicationCount();

        if (appCount == 0)
        {
            TestContext.Out.WriteLine("ℹ️  No applications enabled. Functional tests require seeded app data.");
            return;
        }

        var names = await _applicationListPage.GetApplicationNames();
        _applicationName = names[0];
        TestContext.Out.WriteLine($"📌 Using application: {_applicationName}");

        // Extract application ID from the link href
        var linkLoc = _page.Locator("table.govuk-table tbody tr td a.govuk-link").First;
        var href    = await linkLoc.GetAttributeAsync("href") ?? "";
        var parts   = href.Split('/');
        _applicationId = parts.LastOrDefault() ?? "";
        TestContext.Out.WriteLine($"📌 Application ID: {_applicationId}");
    }

    [Category("ApplicationRegistry")]
    [Test]
    public async Task ApplicationDetailPage_ShowsRolesAndPermissionsTabs()
    {
        TestContext.Out.WriteLine("🔹 Verify application detail page renders Roles/Permissions tabs");
        if (string.IsNullOrEmpty(_applicationId))
        {
            Assert.Ignore("No application available — skipping (no seeded data)");
            return;
        }

        var detailUrl = $"{_baseUrl}/organisation/{_organisationId}/applications/{_applicationId}";
        await _page.GotoAsync(detailUrl);
        await _page.WaitForSelectorAsync("h1.govuk-heading-xl");

        var heading = await _applicationDetailPage.GetHeading();
        Assert.That(heading, Does.Contain(_applicationName));
        TestContext.Out.WriteLine($"✅ Heading: {heading}");

        Assert.That(await _applicationDetailPage.RolesTabIsVisible(),       Is.True, "Roles tab should be visible");
        Assert.That(await _applicationDetailPage.PermissionsTabIsVisible(), Is.True, "Permissions tab should be visible");
        TestContext.Out.WriteLine("✅ Both Roles and Permissions tabs are visible");

        var roleCount = await _applicationDetailPage.GetRoleCount();
        TestContext.Out.WriteLine($"✅ Roles defined: {roleCount}");

        var roleNames = await _applicationDetailPage.GetRoleNames();
        TestContext.Out.WriteLine($"  Role names: {string.Join(", ", roleNames)}");

        Assert.That(await _applicationDetailPage.ManageUsersButtonIsVisible(), Is.True,
            "'Manage user assignments' button should be visible");
        TestContext.Out.WriteLine("✅ 'Manage user assignments' button is visible");
    }

    [Category("ApplicationRegistry")]
    [Test]
    public async Task ApplicationDetailPage_PermissionsTab_LoadsCorrectly()
    {
        TestContext.Out.WriteLine("🔹 Verify Permissions tab is clickable and shows permission data");
        if (string.IsNullOrEmpty(_applicationId))
        {
            Assert.Ignore("No application available — skipping");
            return;
        }

        var detailUrl = $"{_baseUrl}/organisation/{_organisationId}/applications/{_applicationId}";
        await _page.GotoAsync(detailUrl);
        await _page.WaitForSelectorAsync("h1.govuk-heading-xl");

        await _applicationDetailPage.ClickPermissionsTab();
        TestContext.Out.WriteLine("✅ Permissions tab clicked");

        // Verify permissions panel is visible
        var permPanel = _page.Locator("#permissions");
        Assert.That(await permPanel.IsVisibleAsync(), Is.True, "Permissions panel should be visible after click");
        TestContext.Out.WriteLine("✅ Permissions panel is visible");
    }

    [Category("ApplicationRegistry")]
    [Test]
    public async Task UserAssignmentsPage_LoadsAndShowsAssignUserButton()
    {
        TestContext.Out.WriteLine("🔹 Verify user assignments page loads with Assign user button");
        if (string.IsNullOrEmpty(_applicationId))
        {
            Assert.Ignore("No application available — skipping");
            return;
        }

        var url = $"{_baseUrl}/organisation/{_organisationId}/applications/{_applicationId}/user-assignments";
        await _page.GotoAsync(url);
        await _page.WaitForSelectorAsync("h1.govuk-heading-xl");

        var heading = await _userAssignmentsPage.GetHeading();
        Assert.That(heading, Does.Contain(_applicationName));
        TestContext.Out.WriteLine($"✅ Heading: {heading}");

        Assert.That(await _userAssignmentsPage.AssignUserButtonIsVisible(), Is.True,
            "'Assign user' button should be visible");
        TestContext.Out.WriteLine("✅ 'Assign user' button is visible");

        var count = await _userAssignmentsPage.GetAssignmentCount();
        TestContext.Out.WriteLine($"✅ Current assignment count: {count}");
    }

    [Category("ApplicationRegistry")]
    [Test]
    public async Task AssignUserPage_LoadsWithUserPickerAndRoleCheckboxes()
    {
        TestContext.Out.WriteLine("🔹 Verify assign user page loads with user picker and role checkboxes");
        if (string.IsNullOrEmpty(_applicationId))
        {
            Assert.Ignore("No application available — skipping");
            return;
        }

        var assignUrl = $"{_baseUrl}/organisation/{_organisationId}/applications/{_applicationId}/user-assignments/assign";
        await _page.GotoAsync(assignUrl);
        await _page.WaitForSelectorAsync("h1.govuk-heading-xl");

        var heading = await _assignUserPage.GetHeading();
        Assert.That(heading, Does.Contain("Assign user"));
        TestContext.Out.WriteLine($"✅ Heading: {heading}");

        var users = await _assignUserPage.GetAvailableUserOptions();
        TestContext.Out.WriteLine($"✅ Available members in picker: {users.Count}");

        var roles = await _assignUserPage.GetAvailableRoleNames();
        TestContext.Out.WriteLine($"✅ Available roles: {string.Join(", ", roles)}");
        Assert.That(roles, Is.Not.Empty, "At least one role should be available for assignment");
    }

    [Category("ApplicationRegistry")]
    [Test]
    public async Task AssignUserJourney_HappyPath_AssignsThenRevokes()
    {
        TestContext.Out.WriteLine("🔹 Full journey: Assign user → check answers → confirm → revoke");
        if (string.IsNullOrEmpty(_applicationId))
        {
            Assert.Ignore("No application available — skipping");
            return;
        }

        // ── Step 1: Open assign page ──────────────────────────────────────
        var assignUrl = $"{_baseUrl}/organisation/{_organisationId}/applications/{_applicationId}/user-assignments/assign";
        await _page.GotoAsync(assignUrl);
        await _page.WaitForSelectorAsync("h1.govuk-heading-xl");

        // Get available users
        var users = await _assignUserPage.GetAvailableUserOptions();
        if (users.Count == 0)
        {
            TestContext.Out.WriteLine("ℹ️  No unassigned members available — skipping full assignment journey");
            Assert.Ignore("No available members to assign");
            return;
        }

        var targetUser = users[0];
        var roles      = await _assignUserPage.GetAvailableRoleNames();
        var targetRole = roles[0];

        TestContext.Out.WriteLine($"  Assigning user: {targetUser}");
        TestContext.Out.WriteLine($"  With role: {targetRole}");

        // ── Step 2: Select user and role ─────────────────────────────────
        await _assignUserPage.SelectUser(targetUser);
        await _assignUserPage.CheckRole(targetRole);
        TestContext.Out.WriteLine("✅ User and role selected");

        // ── Step 3: Continue to check answers ────────────────────────────
        await _assignUserPage.ClickContinue();
        var checkHeading = await _checkAnswersPage.GetHeading();
        Assert.That(checkHeading, Does.Contain("Confirm"));
        TestContext.Out.WriteLine($"✅ Check answers page: {checkHeading}");

        var selectedRoles = await _checkAnswersPage.GetSelectedRoleNames();
        Assert.That(selectedRoles, Contains.Item(targetRole));
        TestContext.Out.WriteLine($"✅ Role confirmed: {string.Join(", ", selectedRoles)}");

        // ── Step 4: Confirm ───────────────────────────────────────────────
        await _checkAnswersPage.ClickConfirm();
        var pageUrl = await _userAssignmentsPage.GetPageUrl();
        Assert.That(pageUrl, Does.Contain("user-assignments"));
        TestContext.Out.WriteLine("✅ Redirected to user assignments after confirm");

        // ── Step 5: Verify user appears in table ──────────────────────────
        var assignmentCount = await _userAssignmentsPage.GetAssignmentCount();
        Assert.That(assignmentCount, Is.GreaterThan(0), "Table should show at least one assignment");
        TestContext.Out.WriteLine($"✅ Assignments now visible: {assignmentCount}");

        // ── Step 6: Revoke the assignment ─────────────────────────────────
        var countBefore = await _userAssignmentsPage.GetAssignmentCount();
        await _userAssignmentsPage.ClickRevoke(rowIndex: 0);
        await _revokePage.ConfirmRevoke();

        var countAfter = await _userAssignmentsPage.GetAssignmentCount();
        TestContext.Out.WriteLine($"✅ Assignment revoked. Count before: {countBefore}, after: {countAfter}");
        Assert.That(countAfter, Is.LessThan(countBefore), "Row count should decrease after revoke");
    }

    [Category("ApplicationRegistry")]
    [Test]
    public async Task AssignUserPage_ValidationError_WhenNoSelectionMade()
    {
        TestContext.Out.WriteLine("🔹 Verify validation fires when Continue clicked with no selection");
        if (string.IsNullOrEmpty(_applicationId))
        {
            Assert.Ignore("No application available — skipping");
            return;
        }

        var assignUrl = $"{_baseUrl}/organisation/{_organisationId}/applications/{_applicationId}/user-assignments/assign";
        await _page.GotoAsync(assignUrl);
        await _page.WaitForSelectorAsync("h1.govuk-heading-xl");

        // Click Continue without selecting anything
        await _page.ClickAsync("button:has-text('Continue')");

        // Should stay on same page with validation error
        var hasError = await _assignUserPage.HasValidationError();
        Assert.That(hasError, Is.True, "Validation error should appear when no selection made");
        TestContext.Out.WriteLine("✅ Validation error shown as expected");

        var currentUrl = await _assignUserPage.GetPageUrl();
        Assert.That(currentUrl, Does.Contain("/assign"), "Should remain on assign page after validation error");
        TestContext.Out.WriteLine("✅ Remained on assign page");
    }

    [Category("ApplicationRegistry")]
    [Test]
    public async Task RevokeConfirmationPage_SelectNo_CancelsRevoke()
    {
        TestContext.Out.WriteLine("🔹 Verify 'No' on revoke confirmation cancels the revoke");
        if (string.IsNullOrEmpty(_applicationId))
        {
            Assert.Ignore("No application available — skipping");
            return;
        }

        // First check if there are any assignments to revoke
        var url = $"{_baseUrl}/organisation/{_organisationId}/applications/{_applicationId}/user-assignments";
        await _page.GotoAsync(url);
        await _page.WaitForSelectorAsync("h1.govuk-heading-xl");

        var count = await _userAssignmentsPage.GetAssignmentCount();
        if (count == 0)
        {
            TestContext.Out.WriteLine("ℹ️  No assignments to test revoke cancellation — skipping");
            Assert.Ignore("No assignments available");
            return;
        }

        var countBefore = count;
        await _userAssignmentsPage.ClickRevoke(rowIndex: 0);

        // Select No and continue
        await _revokePage.SelectNo();
        await _revokePage.ClickContinue();

        // Should redirect back to assignments page
        var pageUrl = await _userAssignmentsPage.GetPageUrl();
        Assert.That(pageUrl, Does.Contain("user-assignments"), "Should redirect back to assignments page");

        // Count should be unchanged
        var countAfter = await _userAssignmentsPage.GetAssignmentCount();
        Assert.That(countAfter, Is.EqualTo(countBefore), "Assignment count should not change when revoke cancelled");
        TestContext.Out.WriteLine($"✅ Revoke cancelled — count unchanged at {countAfter}");
    }
}
