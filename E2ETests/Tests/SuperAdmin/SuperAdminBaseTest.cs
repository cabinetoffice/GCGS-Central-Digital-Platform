using E2ETests.ApiTests;
using E2ETests.Pages;
using E2ETests.Pages.OrganisationDetails;
using E2ETests.Utilities;

namespace E2ETests.Tests.SuperAdmin;

public class SuperAdminBaseTest : BaseTest
{
    protected static string _organisationId;

    // Page Objects
    protected OrganisationDetailsPage _organisationDetailsPage;
    protected OrganisationName _organisationName;

    // TODO: When we have multiple support admin tests later, we will need to implement a similar one time login as BaseTest

    [SetUp]
    public async Task SetupSuperAdminData()
    {
        await Login();
        string accessToken = GetAccessToken();

        string storageKey = "OrganisationDetails_Org";
        await OrganisationApi.CreateOrganisation(accessToken, "OrganisationDetailsOrg", storageKey);
        _organisationId = OrganisationApi.GetOrganisationId(storageKey);
        Console.WriteLine($"📌 Stored Organisation ID for SuperAdmin Tests: {_organisationId}");
        
        await Logout();

        // Login as SuperAdmin user
        await Login(true);
        await DatabaseUtility.MakeUserSuperAdminAsync(SuperAdminEmail);
        await Logout();
        await Login(true);

        _organisationDetailsPage = new OrganisationDetailsPage(_page);
        _organisationName = new OrganisationName(_page);
    }
}