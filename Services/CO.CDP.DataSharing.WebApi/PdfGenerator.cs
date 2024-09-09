using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CO.CDP.DataSharing.WebApi;

public class PdfGenerator : IPdfGenerator
{
    public byte[] GenerateBasicInformationPdf(SharedSupplierInformation supplierInformation)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var basicInformation = supplierInformation.BasicInformation;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("DejaVu Sans"));

                page.Header().Text("Basic Information").FontSize(16).Bold().AlignCenter();

                page.Content().Column(col =>
                {
                    col.Item().Element(container => AddTwoColumnRow(container, "Supplier Type:", basicInformation.SupplierType?.ToString()));
                    col.Item().Element(container => AddTwoColumnRow(container, "Registered Address:", FormatAddress(basicInformation.RegisteredAddress)));
                    col.Item().Element(container => AddTwoColumnRow(container, "Postal Address:", FormatAddress(basicInformation.PostalAddress)));
                    col.Item().Element(container => AddTwoColumnRow(container, "VAT Number:", basicInformation.VatNumber));
                    col.Item().Element(container => AddTwoColumnRow(container, "Website Address:", basicInformation.WebsiteAddress));
                    col.Item().Element(container => AddTwoColumnRow(container, "Email Address:", basicInformation.EmailAddress));
                    col.Item().Element(container => AddTwoColumnRow(container, "Organisation Type:", basicInformation.OrganisationType.ToString()));

                    if (basicInformation.Qualifications.Any())
                    {
                        col.Item().Text("Qualifications:").Bold();
                        foreach (var qualification in basicInformation.Qualifications)
                        {
                            col.Item().Text($"{qualification.Name} awarded by {qualification.AwardedByPersonOrBodyName} on {qualification.DateAwarded:yyyy-MM-dd}");
                        }
                    }

                    if (basicInformation.TradeAssurances.Any())
                    {
                        col.Item().Text("Trade Assurances:").Bold();
                        foreach (var assurance in basicInformation.TradeAssurances)
                        {
                            col.Item().Text($"{assurance.ReferenceNumber} awarded by {assurance.AwardedByPersonOrBodyName} on {assurance.DateAwarded:yyyy-MM-dd}");
                        }
                    }

                    if (basicInformation.LegalForm != null)
                    {
                        col.Item().Text("Legal Form:").Bold();
                        col.Item().Text($"Registered Legal Form: {basicInformation.LegalForm.RegisteredLegalForm}");
                        col.Item().Text($"Law Registered: {basicInformation.LegalForm.LawRegistered}");
                        col.Item().Text($"Registration Date: {basicInformation.LegalForm.RegistrationDate:yyyy-MM-dd}");
                    }
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Generated on: ");
                    x.Span(DateTime.Now.ToString("yyyy-MM-dd")).Bold();
                });
            });
        });

        using (var stream = new MemoryStream())
        {
            document.GeneratePdf(stream);
            return stream.ToArray();
        }
    }

    private void AddTwoColumnRow(IContainer container, string label, string? value)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(label).Bold();
            row.RelativeItem().Text(value ?? "N/A");
        });
    }

    private string FormatAddress(Address? address)
    {
        if (address == null)
            return "N/A";

        return $"{address.StreetAddress}, {address.Locality}, {address.Region}, {address.PostalCode}, {address.CountryName}, {address.Country}";
    }
}