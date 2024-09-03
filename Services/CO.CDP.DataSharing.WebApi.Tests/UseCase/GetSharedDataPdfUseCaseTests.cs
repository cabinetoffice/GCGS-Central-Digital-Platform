using CO.CDP.DataSharing.WebApi;
using CO.CDP.DataSharing.WebApi.DataService;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace DataSharing.Tests.UseCase;
public class GetSharedDataPdfUseCaseTests
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
    private readonly Mock<IDataService> _dataService = new();
    private readonly Mock<IPdfGenerator> _pdfGenerator = new();

    private GetSharedDataPdfUseCase UseCase => new(
        _shareCodeRepository.Object, _pdfGenerator.Object, _dataService.Object);

    [Fact]
    public async Task Execute_ShouldReturnPdfBytes_WhenShareCodeExists()
    {
        var sharecode = "valid-sharecode";
        var sharedConsent = DataSharingFactory.CreateMockSharedConsent();
        var sharedSupplierInformation = new SharedSupplierInformation
        {
            BasicInformation = new BasicInformation()
        };
        var pdfBytes = new byte[] { 1, 2, 3 };

        _shareCodeRepository.Setup(repo => repo.GetByShareCode(sharecode))
            .ReturnsAsync(sharedConsent);

        _dataService.Setup(service => service.GetSharedSupplierInformationAsync(sharedConsent))
            .ReturnsAsync(sharedSupplierInformation);

        _pdfGenerator.Setup(generator => generator.GenerateBasicInformationPdf(sharedSupplierInformation))
            .Returns(pdfBytes);

        var result = await UseCase.Execute(sharecode);

        result.Should().BeEquivalentTo(pdfBytes);
    }

    [Fact]
    public async Task Execute_ShouldThrowSharedConsentNotFoundException_WhenShareCodeDoesNotExist()
    {
        var sharecode = "invalid-sharecode";

        _shareCodeRepository.Setup(repo => repo.GetByShareCode(sharecode))
            .ReturnsAsync((CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent?)null);

        Func<Task> act = async () => await UseCase.Execute(sharecode);

        await act.Should().ThrowAsync<SharedConsentNotFoundException>()
            .WithMessage("Shared Consent not found.");
    }

    [Fact]
    public async Task Execute_ShouldCallDataServiceAndPdfGenerator_WhenShareCodeExists()
    {
        var sharecode = "valid-sharecode";
        var sharedConsent = DataSharingFactory.CreateMockSharedConsent();
        var sharedSupplierInformation = new SharedSupplierInformation
        {
            BasicInformation = new BasicInformation()
        };
        var pdfBytes = new byte[] { 1, 2, 3 };

        _shareCodeRepository.Setup(repo => repo.GetByShareCode(sharecode))
            .ReturnsAsync(sharedConsent);

        _dataService.Setup(service => service.GetSharedSupplierInformationAsync(sharedConsent))
            .ReturnsAsync(sharedSupplierInformation);

        _pdfGenerator.Setup(generator => generator.GenerateBasicInformationPdf(sharedSupplierInformation))
            .Returns(pdfBytes);

        var result = await UseCase.Execute(sharecode);

        _dataService.Verify(service => service.GetSharedSupplierInformationAsync(sharedConsent), Times.Once);
        _pdfGenerator.Verify(generator => generator.GenerateBasicInformationPdf(sharedSupplierInformation), Times.Once);
    }
}
