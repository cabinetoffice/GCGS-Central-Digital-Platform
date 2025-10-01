using E2ETests.ApiTests;
using E2ETests.Pages;

namespace E2ETests.Tests.AdditionalSupplierInformationModules;

public class DataProtectionBaseTest : BaseTest
{
    protected static string _organisationId;

    // Page Objects

    [SetUp]
    public async Task SetupOCCDataProtectionData()
    {
        await base.Setup();

        string accessToken = GetAccessToken();
        string storageKey = "DataProtection_Org";

        await OrganisationApi.CreateOrganisation(accessToken, "OrganisationDetailsOrg", storageKey);
        _organisationId = OrganisationApi.GetOrganisationId(storageKey);
        Console.WriteLine($"📌 Stored Organisation ID for OrganisationDetails Tests: {_organisationId}");

    }
}
