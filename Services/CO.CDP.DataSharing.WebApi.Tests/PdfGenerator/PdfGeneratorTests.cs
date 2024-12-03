using CO.CDP.Localization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Localization;
using Moq;

namespace CO.CDP.DataSharing.WebApi.Tests.Pdf;

public class PdfGeneratorTests
{
    private readonly PdfGenerator _pdfGenerator;
    private readonly Mock<IHtmlLocalizer<FormsEngineResource>> _localizer = new();
    public PdfGeneratorTests()
    {
        _localizer.Setup(l => l[It.IsAny<string>()])
          .Returns((string key) =>
          {
              if (key == "Localized_String")
              {
                  return new LocalizedHtmlString("Localized_String", "Localized string");
              }

              return new LocalizedHtmlString(key, key);
          });

        _pdfGenerator = new PdfGenerator(_localizer.Object);
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
            AttachedDocuments = [],
            AdditionalIdentifiers = []
        };

        var pdfBytes = _pdfGenerator.GenerateBasicInformationPdf(supplierInformation);

        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(0);
    }
}