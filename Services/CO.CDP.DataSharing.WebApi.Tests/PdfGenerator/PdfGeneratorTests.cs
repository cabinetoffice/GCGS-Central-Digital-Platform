using FluentAssertions;

namespace CO.CDP.DataSharing.WebApi.Tests.Pdf;

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
        var supplierInformation = new Model.SharedSupplierInformation
        {
            OrganisationId = Guid.NewGuid(),
            BasicInformation = DataSharingFactory.CreateMockBasicInformation(),
            ConnectedPersonInformation = DataSharingFactory.CreateMockConnectedPersonInformation(),
            FormAnswerSetForPdfs = DataSharingFactory.CreateMockFormAnswerSetForPdfs(),
            AttachedDocuments = []
        };

        var pdfBytes = _pdfGenerator.GenerateBasicInformationPdf(supplierInformation);

        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(0);
    }
}