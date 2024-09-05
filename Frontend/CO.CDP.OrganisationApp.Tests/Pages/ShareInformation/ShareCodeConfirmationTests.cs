using CO.CDP.OrganisationApp.Pages.ShareInformation;
using FluentAssertions;
using Moq;
using WebApiClient = CO.CDP.DataSharing.WebApiClient;


namespace CO.CDP.OrganisationApp.Tests.Pages.ShareInformation;
public class ShareCodeConfirmationTests
{
    private readonly Mock<WebApiClient.IDataSharingClient> _dataSharingApiClientMock;
    private readonly ShareCodeConfirmationModel _pageModel;

    public ShareCodeConfirmationTests()
    {
        _dataSharingApiClientMock = new Mock<WebApiClient.IDataSharingClient>();
        _pageModel = new ShareCodeConfirmationModel(_dataSharingApiClientMock.Object);
    }

    [Fact]
    public void ShareCodeConfirmationModel_ShouldInitializePropertiesCorrectly()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var shareCode = "HDJ2123F";
        _pageModel.OrganisationId = organisationId;
        _pageModel.FormId = formId;
        _pageModel.SectionId = sectionId;
        _pageModel.ShareCode = shareCode;

        _pageModel.OrganisationId.Should().Be(organisationId);
        _pageModel.FormId.Should().Be(formId);
        _pageModel.SectionId.Should().Be(sectionId);
        _pageModel.ShareCode.Should().Be(shareCode);
    }
}