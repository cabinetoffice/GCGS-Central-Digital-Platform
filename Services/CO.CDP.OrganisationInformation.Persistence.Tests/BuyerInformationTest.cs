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

        organisation.BuyerInfo.Should().BeEquivalentTo(new BuyerInformation());
    }

    [Fact]
    public void ItInitialisesBuyerInformationIfPendingBuyerRoleIsPresent()
    {
        var organisation = GivenOrganisation(
            roles: [],
            pendingRoles: [PartyRole.Buyer]
        );

        organisation.UpdateBuyerInformation();

        organisation.BuyerInfo.Should().BeEquivalentTo(new BuyerInformation());
    }

    [Fact]
    public void ItDoesNotInitialiseBuyerInformationIfBuyerRoleIsNotPresent()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Tenderer]
        );

        organisation.UpdateBuyerInformation();

        organisation.BuyerInfo.Should().BeNull();
    }

    [Fact]
    public void ItDoesNotCreateNewBuyerInformationIfItIsAlreadyPresent()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Buyer],
            buyerInformation: GivenBuyerInformation(type: "Buyer type 1")
        );

        organisation.UpdateBuyerInformation();

        organisation.BuyerInfo.As<BuyerInformation>().BuyerType.Should().Be("Buyer type 1");
    }
}