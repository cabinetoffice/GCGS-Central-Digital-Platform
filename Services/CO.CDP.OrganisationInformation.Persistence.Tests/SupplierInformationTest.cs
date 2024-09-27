using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class SupplierInformationTest
{
    [Fact]
    public void ItInitialisesSupplierInformationIfSupplierRoleIsPresent()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Tenderer],
            addresses: []
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.Should().BeEquivalentTo(new Organisation.SupplierInformation
        {
            CompletedRegAddress = false,
            CompletedPostalAddress = false,
            CompletedVat = false,
            CompletedWebsiteAddress = false,
            CompletedEmailAddress = true,
            CompletedOperationType = false,
            CompletedLegalForm = false
        });
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
            roles: [PartyRole.Tenderer],
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
            roles: [PartyRole.Tenderer],
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
            roles: [PartyRole.Tenderer],
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
            roles: [PartyRole.Tenderer],
            identifiers: [GivenIdentifier(scheme: "VAT")]
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedVat.Should().BeTrue();
    }

    [Fact]
    public void ItMarksLegalFormAsCompletedIfGiven()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Tenderer],
            supplierInformation: GivenSupplierInformation(
                legalForm: GivenSupplierLegalForm(registeredLegalForm: "Limited company")
            )
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedLegalForm.Should().BeTrue();
    }


    [Fact]
    public void ItDoesNotRemoveTheCompletedRegAddressFlagIfItWasExplicitlySet()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Tenderer],
            addresses: [],
            supplierInformation: GivenSupplierInformation(completedRegAddress: true)
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedRegAddress.Should().BeTrue();
    }

    [Fact]
    public void ItDoesNotRemoveTheCompletedPostalAddressFlagIfItWasExplicitlySet()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Tenderer],
            addresses: [],
            supplierInformation: GivenSupplierInformation(completedPostalAddress: true)
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedPostalAddress.Should().BeTrue();
    }


    [Fact]
    public void ItDoesNotRemoveTheVatFlagIfItWasExplicitlySet()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Tenderer],
            supplierInformation: GivenSupplierInformation(completedVat: true)
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedVat.Should().BeTrue();
    }

    [Fact]
    public void ItDoesNotRemoveTheCompletedLegalFormFlagIfItWasExplicitlySet()
    {
        var organisation = GivenOrganisation(
            roles: [PartyRole.Tenderer],
            supplierInformation: GivenSupplierInformation(completedLegalForm: true)
        );

        organisation.UpdateSupplierInformation();

        organisation.SupplierInfo.As<Organisation.SupplierInformation>()
            .CompletedLegalForm.Should().BeTrue();
    }
}