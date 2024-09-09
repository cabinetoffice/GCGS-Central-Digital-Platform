using CO.CDP.DataSharing.WebApi;
using CO.CDP.DataSharing.WebApi.Model;
using DataSharing.Tests;

public class PdfGeneratorTests
{
    private readonly PdfGenerator _pdfGenerator;

    public PdfGeneratorTests()
    {
        _pdfGenerator = new PdfGenerator();
    }

    [Fact]
    public void GenerateBasicInformationPdf_ShouldGeneratePdfWithBasicInformation()
    {

        var supplierInformation = new SharedSupplierInformation
        {
            BasicInformation = DataSharingFactory.CreateMockBasicInformation()
        };

        var pdfBytes = _pdfGenerator.GenerateBasicInformationPdf(supplierInformation);

        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
    }
}