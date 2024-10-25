using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.DataSharing.WebApi.DataService;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using FluentAssertions;
using Moq;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class GetSharedDataFileUseCaseTests
{
    private readonly Mock<IDataService> _dataService = new();
    private readonly Mock<IPdfGenerator> _pdfGenerator = new();
    private readonly Mock<IClaimService> _claimService = new();
    private readonly Mock<IFileHostManager> _fileHostManager = new();
    private string[] requiredClaims = [OrganisationPersonScope.Admin, OrganisationPersonScope.Editor, OrganisationPersonScope.Viewer];

    private GetSharedDataFileUseCase UseCase => new(_pdfGenerator.Object, _dataService.Object, _claimService.Object, _fileHostManager.Object);

    [Fact]
    public async Task Execute_ShouldCallDataServiceAndPdfGenerator_WhenShareCodeExists()
    {
        var organisationId = Guid.NewGuid();
        var sharecode = "valid-sharecode";

        var sharedSupplierInformation = new SharedSupplierInformation
        {
            OrganisationId = organisationId,
            BasicInformation = DataSharingFactory.CreateMockBasicInformation(),
            ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation(),
            FormAnswerSetForPdfs = DataSharingFactory.CreateMockFormAnswerSetForPdfs(),
            AttachedDocuments = []
        };

        var pdfBytes = new byte[] { 1, 2, 3 };

        _dataService.Setup(service => service.GetSharedSupplierInformationAsync(sharecode))
            .ReturnsAsync(sharedSupplierInformation);

        _pdfGenerator.Setup(generator => generator.GenerateBasicInformationPdf(sharedSupplierInformation))
            .Returns(new MemoryStream(pdfBytes));
        _claimService.Setup(cs => cs.HaveAccessToOrganisation(organisationId, requiredClaims, It.IsAny<string[]?>()))
            .ReturnsAsync(true);

        var result = await UseCase.Execute(sharecode);

        result.Should().NotBeNull();
        result.As<SharedDataFile>().FileName.Should().BeEquivalentTo("valid-sharecode.pdf");
        result.As<SharedDataFile>().Content.Should().BeEquivalentTo(pdfBytes);
        result.As<SharedDataFile>().ContentType.Should().BeEquivalentTo("application/pdf");
    }

    [Fact]
    public async Task Execute_WhenDoesNotHaveAccessToOrganisation_ThrowsUserUnauthorizedException()
    {
        var organisationId = Guid.NewGuid();
        var sharecode = "valid-sharecode";
        string[] invalidscope = [OrganisationPersonScope.Responder];

        _dataService.Setup(service => service.GetSharedSupplierInformationAsync(sharecode))
            .ReturnsAsync(new SharedSupplierInformation
            {
                OrganisationId = organisationId,
                BasicInformation = DataSharingFactory.CreateMockBasicInformation(),
                ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation(),
                FormAnswerSetForPdfs = DataSharingFactory.CreateMockFormAnswerSetForPdfs(),
                AttachedDocuments = []
            });

        _claimService.Setup(cs => cs.HaveAccessToOrganisation(organisationId, invalidscope, null))
            .ReturnsAsync(true);

        var act = async () => await UseCase.Execute(sharecode);

        await act.Should().ThrowAsync<UserUnauthorizedException>();
    }

    [Fact]
    public async Task DataService_ShouldReturnBasicInformationWithOrganisationName()
    {
        var organisationId = Guid.NewGuid();
        var sharecode = "valid-sharecode";
        var expectedOrganisationName = "Organisation Name";

        var sharedSupplierInformation = new SharedSupplierInformation
        {
            OrganisationId = organisationId,
            BasicInformation = DataSharingFactory.CreateMockBasicInformation(),
            ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation(),
            FormAnswerSetForPdfs = DataSharingFactory.CreateMockFormAnswerSetForPdfs(),
            AttachedDocuments = []
        };

        _dataService.Setup(service => service.GetSharedSupplierInformationAsync(sharecode))
            .ReturnsAsync(sharedSupplierInformation);

        var result = await _dataService.Object.GetSharedSupplierInformationAsync(sharecode);

        result.Should().NotBeNull();
        result.BasicInformation.OrganisationName.Should().Be(expectedOrganisationName);
    }
}