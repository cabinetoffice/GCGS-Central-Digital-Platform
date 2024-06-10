using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class SupplierInformationTest
{
    [Fact]
    public void ItInitialisesSupplierInformationIfSupplierRoleIsPresent()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Supplier],
            addresses: []
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.Should().BeEquivalentTo(new Organisation.SupplierInformation());
    }

    [Fact]
    public void ItDoesNotInitialiseSupplierInformationIfSupplierRoleIsNotPresent()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Buyer]
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.Should().BeNull();
    }

    [Fact]
    public void ItDoesNotCreateNewSupplierInformationIfItIsAlreadyPresent()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Supplier],
            supplierInformation: GivenSupplierInformation(type: SupplierType.Individual)
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .SupplierType.Should().Be(SupplierType.Individual);
    }

    [Fact]
    public void ItMarksRegistrationAddressAsCompletedInSupplierInformationIfGiven()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Supplier],
            addresses: [GivenOrganisationAddress(type: AddressType.Registered)]
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedRegAddress.Should().BeTrue();
    }

    [Fact]
    public void ItMarksPostalAddressAsCompletedInSupplierInformationIfGiven()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Supplier],
            addresses: [GivenOrganisationAddress(type: AddressType.Postal)]
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedPostalAddress.Should().BeTrue();
    }

    [Fact]
    public void ItMarksVatAsCompletedInSupplierInformationIfGiven()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Supplier],
            identifiers: [GivenIdentifier(scheme: "VAT")]
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedVat.Should().BeTrue();
    }

    [Fact]
    public void ItMarksQualificationAsCompletedIfGiven()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Supplier],
            supplierInformation: GivenSupplierInformation(
                qualifications: [GivenSupplierQualification(name: "Qualification 1")]
            )
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedQualification.Should().BeTrue();
    }

    [Fact]
    public void ItMarksTradeAssuranceAsCompletedIfGiven()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Supplier],
            supplierInformation: GivenSupplierInformation(
                tradeAssurances: [GivenSupplierTradeAssurance()]
            )
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedTradeAssurance.Should().BeTrue();
    }
}