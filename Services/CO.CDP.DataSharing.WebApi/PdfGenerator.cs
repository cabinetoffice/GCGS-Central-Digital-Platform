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
        var connectedPersonInformationTask = supplierInformation.ConnectedPersonInformation;
        var connectedPersonInformation = connectedPersonInformationTask.Result;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("DejaVu Sans"));


                page.Header().Text("Supplier Information").FontSize(16).Bold().AlignCenter();

                page.Content().Column(col =>
                {
                    AddBasicInformationSection(col, basicInformation);
                    AddConnectedPersonInformationSection(col, connectedPersonInformation);
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

    private void AddBasicInformationSection(ColumnDescriptor col, BasicInformation basicInformation)
    {
        col.Item().Text("Basic Information").Bold().FontSize(14);
        col.Item().PaddingBottom(10);

        if (!string.IsNullOrEmpty(basicInformation.SupplierType?.ToString()))
            col.Item().Element(container => AddTwoColumnRow(container, "Supplier Type:", basicInformation.SupplierType?.ToString()));

        if (basicInformation.RegisteredAddress != null)
            col.Item().Element(container => AddTwoColumnRow(container, "Registered Address:", FormatAddress(basicInformation.RegisteredAddress)));

        if (basicInformation.PostalAddress != null)
            col.Item().Element(container => AddTwoColumnRow(container, "Postal Address:", FormatAddress(basicInformation.PostalAddress)));

        if (!string.IsNullOrEmpty(basicInformation.VatNumber))
            col.Item().Element(container => AddTwoColumnRow(container, "VAT Number:", basicInformation.VatNumber));

        if (!string.IsNullOrEmpty(basicInformation.WebsiteAddress))
            col.Item().Element(container => AddTwoColumnRow(container, "Website Address:", basicInformation.WebsiteAddress));

        if (!string.IsNullOrEmpty(basicInformation.EmailAddress))
            col.Item().Element(container => AddTwoColumnRow(container, "Email Address:", basicInformation.EmailAddress));

        if (!string.IsNullOrEmpty(basicInformation.OrganisationType.ToString()))
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

        col.Item().LineHorizontal(1);
        col.Item().PaddingBottom(10);
    }

    private void AddConnectedPersonInformationSection(ColumnDescriptor col, List<ConnectedPersonInformation> connectedPersons)
    {
        col.Item().Text("Connected Person Information").Bold().FontSize(14);
        col.Item().PaddingBottom(10);

        if (connectedPersons.Any())
        {
            foreach (var person in connectedPersons)
            {
                col.Item().LineHorizontal(1);

                if (!string.IsNullOrEmpty(person.PersonId.ToString()))
                    col.Item().Element(container => AddTwoColumnRow(container, "Person ID:", person.PersonId.ToString()));

                if (!string.IsNullOrEmpty(person.FirstName))
                    col.Item().Element(container => AddTwoColumnRow(container, "First Name:", person.FirstName));

                if (!string.IsNullOrEmpty(person.LastName))
                    col.Item().Element(container => AddTwoColumnRow(container, "Last Name:", person.LastName));

                if (!string.IsNullOrEmpty(person.Nationality))
                    col.Item().Element(container => AddTwoColumnRow(container, "Nationality:", person.Nationality));

                if (person.DateOfBirth != null)
                    col.Item().Element(container => AddTwoColumnRow(container, "Date of Birth:", person.DateOfBirth?.ToString("yyyy-MM-dd")));

                if (!string.IsNullOrEmpty(person.PersonType.ToString()))
                    col.Item().Element(container => AddTwoColumnRow(container, "Person Type:", person.PersonType.ToString()));

                if (!string.IsNullOrEmpty(person.Category.ToString()))
                    col.Item().Element(container => AddTwoColumnRow(container, "Category:", person.Category.ToString()));

                if (!string.IsNullOrEmpty(person.ResidentCountry))
                    col.Item().Element(container => AddTwoColumnRow(container, "Resident Country:", person.ResidentCountry));

                var registeredAddress = person.Addresses.FirstOrDefault(a => a.Type == AddressType.Registered);
                var postalAddress = person.Addresses.FirstOrDefault(a => a.Type == AddressType.Postal);

                if (registeredAddress != null)
                {
                    col.Item().Text("Registered Address:").Bold();
                    col.Item().Text(FormatAddress(new Address
                    {
                        StreetAddress = registeredAddress.StreetAddress,
                        Locality = registeredAddress.Locality,
                        Region = registeredAddress.Region,
                        PostalCode = registeredAddress.PostalCode,
                        CountryName = registeredAddress.CountryName,
                        Type = AddressType.Registered,
                        Country = registeredAddress.CountryName
                    }));
                }

                if (postalAddress != null)
                {
                    col.Item().Text("Postal Address:").Bold();
                    col.Item().Text(FormatAddress(new Address
                    {
                        StreetAddress = postalAddress.StreetAddress,
                        Locality = postalAddress.Locality,
                        Region = postalAddress.Region,
                        PostalCode = postalAddress.PostalCode,
                        CountryName = postalAddress.CountryName,
                        Type = AddressType.Postal,
                        Country = postalAddress.CountryName
                    }));
                }

                if (person.ControlConditions.Any())
                {
                    col.Item().Text("Control Conditions:").Bold();
                    foreach (var condition in person.ControlConditions)
                    {
                        col.Item().Text(condition);
                    }
                }

                if (person.IndividualTrust != null)
                {
                    col.Item().Text("Individual Trust Information:").Bold();
                    if (!string.IsNullOrEmpty(person.IndividualTrust.FirstName))
                        col.Item().Element(container => AddTwoColumnRow(container, "First Name:", person.IndividualTrust.FirstName));
                    if (!string.IsNullOrEmpty(person.IndividualTrust.LastName))
                        col.Item().Element(container => AddTwoColumnRow(container, "Last Name:", person.IndividualTrust.LastName));
                    if (!string.IsNullOrEmpty(person.IndividualTrust.Nationality))
                        col.Item().Element(container => AddTwoColumnRow(container, "Nationality:", person.IndividualTrust.Nationality));
                    if (person.IndividualTrust.DateOfBirth != null)
                        col.Item().Element(container => AddTwoColumnRow(container, "Date of Birth:", person.IndividualTrust.DateOfBirth?.ToString("yyyy-MM-dd")));
                }

                if (person.Organisation != null)
                {
                    col.Item().Text("Organisation Information:").Bold();
                    if (!string.IsNullOrEmpty(person.Organisation.Name))
                        col.Item().Element(container => AddTwoColumnRow(container, "Name:", person.Organisation.Name));
                    if (!string.IsNullOrEmpty(person.Organisation.RegisteredLegalForm))
                        col.Item().Element(container => AddTwoColumnRow(container, "Registered Legal Form:", person.Organisation.RegisteredLegalForm));
                    if (!string.IsNullOrEmpty(person.Organisation.LawRegistered))
                        col.Item().Element(container => AddTwoColumnRow(container, "Law Registered:", person.Organisation.LawRegistered));
                }
            }
        }
        else
        {
            col.Item().Text("No Connected Person Information available.");
        }

        col.Item().LineHorizontal(1);
        col.Item().PaddingBottom(10);
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

        return $"{address.StreetAddress}, {address.Locality}, {address.Region}, {address.PostalCode}, {address.CountryName}";
    }
}