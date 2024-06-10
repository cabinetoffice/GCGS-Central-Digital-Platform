using FluentAssertions;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

using static EntityFactory;

public class BuyerInformationTest
{
    [Fact]
    public void ItInitialisesBuyerInformationIfBuyerRoleIsPresent()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Buyer]
        );

        organisation.UpdateBuyerInformation();

        organisation.BuyerInfo.Should().BeEquivalentTo(new Organisation.BuyerInformation
        {
            BuyerType = ""
        });
    }

    [Fact]
    public void ItDoesNotInitialiseBuyerInformationIfBuyerRoleIsNotPresent()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Supplier]
        );

        organisation.UpdateBuyerInformation();

        organisation.BuyerInfo.Should().BeNull();
    }
}