using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace CO.CDP.DataSharing.WebApi;

public class PdfGenerator : IPdfGenerator
{
    public byte[] GenerateBasicInformationPdf(SharedSupplierInformation supplierInformation)
    {
        var basicInformation = supplierInformation.BasicInformation;
        var document = new Document();
        var section = document.AddSection();

        var title = section.AddParagraph("Basic Information");
        title.Format.Font.Size = 16;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = 10;

        AddTwoColumnRow(section, "Supplier Type:", basicInformation.SupplierType?.ToString());
        AddTwoColumnRow(section, "Registered Address:", FormatAddress(basicInformation.RegisteredAddress));
        AddTwoColumnRow(section, "Postal Address:", FormatAddress(basicInformation.PostalAddress));
        AddTwoColumnRow(section, "VAT Number:", basicInformation.VatNumber);
        AddTwoColumnRow(section, "Website Address:", basicInformation.WebsiteAddress);
        AddTwoColumnRow(section, "Email Address:", basicInformation.EmailAddress);
        AddTwoColumnRow(section, "Organisation Type:", basicInformation.OrganisationType.ToString());

        if (basicInformation.Qualifications.Any())
        {
            section.AddParagraph("Qualifications:").Format.Font.Bold = true;
            foreach (var qualification in basicInformation.Qualifications)
            {
                section.AddParagraph($"{qualification.Name} awarded by {qualification.AwardedByPersonOrBodyName} on {qualification.DateAwarded:yyyy-MM-dd}");
            }
        }

        if (basicInformation.TradeAssurances.Any())
        {
            section.AddParagraph("Trade Assurances:").Format.Font.Bold = true;
            foreach (var assurance in basicInformation.TradeAssurances)
            {
                section.AddParagraph($"{assurance.ReferenceNumber} awarded by {assurance.AwardedByPersonOrBodyName} on {assurance.DateAwarded:yyyy-MM-dd}");
            }
        }

        if (basicInformation.LegalForm != null)
        {
            section.AddParagraph("Legal Form:").Format.Font.Bold = true;
            section.AddParagraph($"Registered Legal Form: {basicInformation.LegalForm.RegisteredLegalForm}");
            section.AddParagraph($"Law Registered: {basicInformation.LegalForm.LawRegistered}");
            section.AddParagraph($"Registration Date: {basicInformation.LegalForm.RegistrationDate:yyyy-MM-dd}");
        }

        var pdfRenderer = new PdfDocumentRenderer { Document = document };
        pdfRenderer.RenderDocument();

        using (var stream = new MemoryStream())
        {
            pdfRenderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }
    }

    private void AddTwoColumnRow(Section section, string label, string? value)
    {
        var row = section.AddParagraph();
        row.AddFormattedText(label, TextFormat.Bold);
        row.AddTab();
        row.AddText(value ?? "N/A");
    }

    private string FormatAddress(Address? address)
    {
        if (address == null)
            return "N/A";

        return $"{address.StreetAddress}, {address.Locality}, {address.Region}, {address.PostalCode}, {address.CountryName}, {address.Country}";
    }
}

