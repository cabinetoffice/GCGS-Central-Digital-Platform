using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Tests.TestData;
using CO.CDP.OrganisationApp.WebApiClients;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests.WebApiClients;

public class OrganisationExtensionsHasFeatureTests
{
    [Fact]
    public void HasFeature_WhenDetailsIsNull_ReturnsFalse()
    {
        var organisation = OrganisationFactory.CreateOrganisation() with { Details = null! };

        organisation.HasFeature(OrganisationFeatures.AiTool).Should().BeFalse();
    }

    [Fact]
    public void HasFeature_WhenFeaturesIsNull_ReturnsFalse()
    {
        var organisation = OrganisationFactory.CreateOrganisation(
            details: new Details(null, null, null, [], null, null, null, null));

        organisation.HasFeature(OrganisationFeatures.AiTool).Should().BeFalse();
    }

    [Fact]
    public void HasFeature_WhenFeaturesIsEmpty_ReturnsFalse()
    {
        var organisation = OrganisationFactory.CreateOrganisation(features: []);

        organisation.HasFeature(OrganisationFeatures.AiTool).Should().BeFalse();
    }

    [Fact]
    public void HasFeature_WhenFeatureIsPresent_ReturnsTrue()
    {
        var organisation = OrganisationFactory.CreateOrganisation(features: ["other-feature", OrganisationFeatures.AiTool]);

        organisation.HasFeature(OrganisationFeatures.AiTool).Should().BeTrue();
    }

    [Fact]
    public void HasFeature_WhenFeatureIsNotPresent_ReturnsFalse()
    {
        var organisation = OrganisationFactory.CreateOrganisation(features: ["other-feature"]);

        organisation.HasFeature(OrganisationFeatures.AiTool).Should().BeFalse();
    }

    [Fact]
    public void HasFeature_IsCaseSensitive()
    {
        var organisation = OrganisationFactory.CreateOrganisation(features: ["AI-TOOL"]);

        organisation.HasFeature(OrganisationFeatures.AiTool).Should().BeFalse();
    }
}
