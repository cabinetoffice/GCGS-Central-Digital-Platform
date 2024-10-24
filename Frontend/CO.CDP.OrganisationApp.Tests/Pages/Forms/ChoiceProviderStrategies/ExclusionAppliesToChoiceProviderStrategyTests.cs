using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using FluentAssertions;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms.ChoiceProviderStrategies;
public class ExclusionAppliesToChoiceProviderStrategyTests
{
    public IChoiceProviderStrategy choiceProviderStrategy;
    public Mock<IOrganisationClient> mockOrganisationClient;
    public Mock<IUserInfoService> mockUserInfoService;
    public ExclusionAppliesToChoiceProviderStrategyTests()
    {
        mockOrganisationClient = new Mock<IOrganisationClient>();
        mockUserInfoService = new Mock<IUserInfoService>();

        var orgId = new Guid("0984665e-4a4b-4e9a-88d8-cd97a6fae13e");

        mockUserInfoService.Setup(u => u.GetOrganisationId()).Returns(orgId);

        mockOrganisationClient
            .Setup(oc => oc.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new CO.CDP.Organisation.WebApiClient.Organisation(null, null, null, null, orgId, null, "Org name", []));

        choiceProviderStrategy = new ExclusionAppliesToChoiceProviderStrategy(mockUserInfoService.Object, mockOrganisationClient.Object);
    }

    [Fact]
    public async Task RenderMethodShouldRenderCorrectly_WhenGivenAConnectedOrganisation()
    {
        mockOrganisationClient
            .Setup(oc => oc.GetConnectedEntityAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(ExampleConnectedOrganisation());

        var result = await choiceProviderStrategy.RenderOption(new Models.FormAnswer() { JsonValue = "{\"id\": \"1083f914-a4c0-4e0b-8650-0b5e46696666\", \"type\": \"connected-entity\"}" });

        result.Should().Be("Org name");
    }

    [Fact]
    public async Task RenderMethodShouldRenderCorrectly_WhenGivenAConnectedTrustee()
    {
        mockOrganisationClient
            .Setup(oc => oc.GetConnectedEntityAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(ExampleConnectedIndividual(ConnectedEntityType.TrustOrTrustee, ConnectedIndividualAndTrustCategory.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust, ConnectedPersonType.TrustOrTrustee));

        var result = await choiceProviderStrategy.RenderOption(new Models.FormAnswer() { JsonValue = "{\"id\": \"1083f914-a4c0-4e0b-8650-0b5e46696666\", \"type\": \"connected-entity\"}" });

        result.Should().Be("First name Last name");
    }

    [Fact]
    public async Task RenderMethodShouldRenderCorrectly_WhenGivenAConnectedIndividual()
    {
        mockOrganisationClient
            .Setup(oc => oc.GetConnectedEntityAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(ExampleConnectedIndividual(ConnectedEntityType.Individual, ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForIndividual, ConnectedPersonType.Individual));

        var result = await choiceProviderStrategy.RenderOption(new Models.FormAnswer() { JsonValue = "{\"id\": \"1083f914-a4c0-4e0b-8650-0b5e46696666\", \"type\": \"connected-entity\"}" });

        result.Should().Be("First name Last name");
    }

    internal ConnectedEntity ExampleConnectedIndividual(ConnectedEntityType connectedEntityType, ConnectedIndividualAndTrustCategory connectedIndividualAndTrustCategory, ConnectedPersonType connectedPersonType)
    {
        return new ConnectedEntity([], "asd", null, ConnectedEntityType.TrustOrTrustee, false, new Guid(), new ConnectedIndividualTrust(ConnectedIndividualAndTrustCategory.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust, ConnectedPersonType.TrustOrTrustee, [], null, "First name", 4, "Last name", null, new Guid(), null), null, null, null, "Register name", null);
    }

    internal ConnectedEntity ExampleConnectedOrganisation()
    {
        return new ConnectedEntity([], "asd", null, ConnectedEntityType.Organisation, false, new Guid(), null, new ConnectedOrganisation(ConnectedOrganisationCategory.AnyOtherOrganisationWithSignificantInfluenceOrControl, [], 3, null, null, "Org name", new Guid(), null), null, null, "Register name", null);
    }
}
