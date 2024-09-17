using CO.CDP.DataSharing.WebApi;
using CO.CDP.DataSharing.WebApi.DataService;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.Tests;
using CO.CDP.DataSharing.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace DataSharing.Tests.UseCase;

public class GetSharedDataPdfUseCaseTests
{
    private readonly Mock<IDataService> _dataService = new();
    private readonly Mock<IPdfGenerator> _pdfGenerator = new();

    private GetSharedDataPdfUseCase UseCase => new(_pdfGenerator.Object, _dataService.Object);

    [Fact]
    public async Task Execute_ShouldReturnPdfBytes_WhenShareCodeExists()
    {
        var sharecode = "valid-sharecode";
        var sharedSupplierInformation = new SharedSupplierInformation
        {
            BasicInformation = DataSharingFactory.CreateMockBasicInformation(),
            ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation()
        };

        var pdfBytes = new byte[] { 1, 2, 3 };

        _dataService.Setup(service => service.GetSharedSupplierInformationAsync(sharecode))
            .ReturnsAsync(sharedSupplierInformation);

        _pdfGenerator.Setup(generator => generator.GenerateBasicInformationPdf(sharedSupplierInformation))
            .Returns(pdfBytes);

        var result = await UseCase.Execute(sharecode);

        result.Should().BeEquivalentTo(pdfBytes);
    }

    [Fact]
    public async Task Execute_ShouldCallDataServiceAndPdfGenerator_WhenShareCodeExists()
    {
        var sharecode = "valid-sharecode";

        var sharedSupplierInformation = new SharedSupplierInformation
        {
            BasicInformation = DataSharingFactory.CreateMockBasicInformation(),
            ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation()
        };

        var pdfBytes = new byte[] { 1, 2, 3 };

        _dataService.Setup(service => service.GetSharedSupplierInformationAsync(sharecode))
            .ReturnsAsync(sharedSupplierInformation);

        _pdfGenerator.Setup(generator => generator.GenerateBasicInformationPdf(sharedSupplierInformation))
            .Returns(pdfBytes);

        var result = await UseCase.Execute(sharecode);

        result.Should().BeEquivalentTo(pdfBytes);
    }
}