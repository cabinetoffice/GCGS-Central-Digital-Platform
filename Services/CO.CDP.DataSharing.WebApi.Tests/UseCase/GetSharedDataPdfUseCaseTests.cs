using CO.CDP.Authentication;
using CO.CDP.DataSharing.WebApi;
using CO.CDP.DataSharing.WebApi.DataService;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.Tests;
using CO.CDP.DataSharing.WebApi.UseCase;
using FluentAssertions;
using Moq;
using static CO.CDP.Authentication.Constants;

namespace DataSharing.Tests.UseCase;

public class GetSharedDataPdfUseCaseTests
{
    private readonly Mock<IDataService> _dataService = new();
    private readonly Mock<IPdfGenerator> _pdfGenerator = new();
    private readonly Mock<IClaimService> _claimService = new();
    private string[] requiredClaims = [OrganisationPersonScope.Admin, OrganisationPersonScope.Editor, OrganisationPersonScope.Viewer];

    private GetSharedDataPdfUseCase UseCase => new(_pdfGenerator.Object, _dataService.Object, _claimService.Object);

    [Fact]
    public async Task Execute_ShouldReturnPdfBytes_WhenShareCodeExists()
    {
        var organisationId = Guid.NewGuid();
        var sharecode = "valid-sharecode";
        var sharedSupplierInformation = new SharedSupplierInformation
        {
            OrganisationId = organisationId,
            BasicInformation = DataSharingFactory.CreateMockBasicInformation(),
            ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation()
        };

        var pdfBytes = new byte[] { 1, 2, 3 };

        _dataService.Setup(service => service.GetSharedSupplierInformationAsync(sharecode))
            .ReturnsAsync(sharedSupplierInformation);

        _pdfGenerator.Setup(generator => generator.GenerateBasicInformationPdf(sharedSupplierInformation))
            .Returns(pdfBytes);
        _claimService.Setup(cs => cs.HaveAccessToOrganisation(organisationId, requiredClaims))
            .ReturnsAsync(true);

        var result = await UseCase.Execute(sharecode);

        result.Should().BeEquivalentTo(pdfBytes);
    }

    [Fact]
    public async Task Execute_ShouldCallDataServiceAndPdfGenerator_WhenShareCodeExists()
    {
        var organisationId = Guid.NewGuid();
        var sharecode = "valid-sharecode";

        var sharedSupplierInformation = new SharedSupplierInformation
        {
            OrganisationId = organisationId,
            BasicInformation = DataSharingFactory.CreateMockBasicInformation(),
            ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation()
        };

        var pdfBytes = new byte[] { 1, 2, 3 };

        _dataService.Setup(service => service.GetSharedSupplierInformationAsync(sharecode))
            .ReturnsAsync(sharedSupplierInformation);

        _pdfGenerator.Setup(generator => generator.GenerateBasicInformationPdf(sharedSupplierInformation))
            .Returns(pdfBytes);
        _claimService.Setup(cs => cs.HaveAccessToOrganisation(organisationId, requiredClaims))
            .ReturnsAsync(true);

        var result = await UseCase.Execute(sharecode);

        result.Should().BeEquivalentTo(pdfBytes);
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
                ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation()
            });

        _claimService.Setup(cs => cs.HaveAccessToOrganisation(organisationId, invalidscope))
            .ReturnsAsync(true);

        var act = async () => await UseCase.Execute(sharecode);

        await act.Should().ThrowAsync<UserUnauthorizedException>();
    }
}