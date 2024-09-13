using CO.CDP.DataSharing.WebApi;
using DataSharing.Tests;

public class PdfGeneratorTests
{
    private readonly PdfGenerator _pdfGenerator;

    public PdfGeneratorTests()
    {
        _pdfGenerator = new PdfGenerator();
    }

    [Fact]
    public void GenerateBasicInformationPdf_ShouldGeneratePdfWithAllInformation()
    {

        var supplierInformation = new CO.CDP.DataSharing.WebApi.Model.SharedSupplierInformation
        {
            BasicInformation = DataSharingFactory.CreateMockBasicInformation(),
            ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation(),
        };

        var pdfBytes = _pdfGenerator.GenerateBasicInformationPdf(supplierInformation);

        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
    }
}