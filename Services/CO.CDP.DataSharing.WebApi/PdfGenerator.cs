using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CO.CDP.DataSharing.WebApi;

public class PdfGenerator : IPdfGenerator
{
    public Stream GenerateBasicInformationPdf(SharedSupplierInformation supplierInformation)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var basicInformation = supplierInformation.BasicInformation;
        var connectedPersonInformation = supplierInformation.ConnectedPersonInformation;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("DejaVu Sans"));


                page.Header().Text("Supplier Information").FontSize(14).Bold().AlignCenter();

                page.Content().Column(col =>
                {
                    AddBasicInformationSection(col, basicInformation);
                    AddConnectedPersonInformationSection(col, connectedPersonInformation);
                    AddFormSections(col, supplierInformation.FormAnswerSetForPdfs);
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Generated on: ");
                    x.Span(DateTime.Now.ToString("dd-MM-yyyy")).Bold();
                });
            });
        });

        var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream;
    }

    private void AddFormSections(ColumnDescriptor col, IEnumerable<FormAnswerSetForPdf> formAnswerSet)
    {
        foreach (var answerSet in formAnswerSet)
        {
            col.Item().Text(answerSet.SectionName).Bold().FontSize(12);
            col.Item().PaddingBottom(10);

            if (answerSet.QuestionAnswers != null)
            {
                foreach (var qa in answerSet.QuestionAnswers)
                {
                    col.Item().Element(container => AddTwoColumnRow(container, qa.Item1, qa.Item2));
                }

                col.Item().PaddingBottom(10);
            }

            col.Item().LineHorizontal(1);
            col.Item().PaddingBottom(10);
        }
    }

    private void AddBasicInformationSection(ColumnDescriptor col, BasicInformation basicInformation)
    {
        col.Item().Text("Basic Information").Bold().FontSize(12);
        col.Item().PaddingBottom(10);

        if (!string.IsNullOrEmpty(basicInformation.SupplierType?.ToString()))
            col.Item().Element(container => AddTwoColumnRow(container, "Supplier type:", basicInformation.SupplierType?.ToString()));

        if (!string.IsNullOrEmpty(basicInformation.OrganisationName?.ToString()))
            col.Item().Element(container => AddTwoColumnRow(container, "Organisation name:", basicInformation.OrganisationName?.ToString()));

        if (basicInformation.RegisteredAddress != null)
            col.Item().Element(container => AddTwoColumnRow(container, "Registered address:", FormatAddress(basicInformation.RegisteredAddress)));

        if (basicInformation.PostalAddress != null)
            col.Item().Element(container => AddTwoColumnRow(container, "Postal address:", FormatAddress(basicInformation.PostalAddress)));

        if (!string.IsNullOrEmpty(basicInformation.VatNumber))
            col.Item().Element(container => AddTwoColumnRow(container, "VAT number:", basicInformation.VatNumber));

        if (!string.IsNullOrEmpty(basicInformation.WebsiteAddress))
            col.Item().Element(container => AddTwoColumnRow(container, "Website address:", basicInformation.WebsiteAddress));

        if (!string.IsNullOrEmpty(basicInformation.EmailAddress))
            col.Item().Element(container => AddTwoColumnRow(container, "Email address:", basicInformation.EmailAddress));

        if (!string.IsNullOrEmpty(basicInformation.OrganisationType.ToString()))
            col.Item().Element(container => AddTwoColumnRow(container, "Organisation type:", basicInformation.OrganisationType.ToString()));

        if (basicInformation.LegalForm != null)
        {
            col.Item().Element(container =>
            {
                AddTwoColumnRow(container, "Legal form:",
                    $"Registered legal form: {basicInformation.LegalForm.RegisteredLegalForm}\n" +
                    $"Law registered: {basicInformation.LegalForm.LawRegistered}\n" +
                    $"Registration date: {basicInformation.LegalForm.RegistrationDate:dd-MM-yyyy}");
            });
        }

        col.Item().LineHorizontal(1);
        col.Item().PaddingBottom(10);
    }

    private void AddConnectedPersonInformationSection(ColumnDescriptor col, List<ConnectedPersonInformation> connectedPersons)
    {
        col.Item().Text("Connected Person Information").Bold().FontSize(12);
        col.Item().PaddingBottom(10);

        if (connectedPersons.Any())
        {
            foreach (var person in connectedPersons)
            {
                col.Item().LineHorizontal(1);

                if (!string.IsNullOrEmpty(person.FirstName))
                    col.Item().Element(container => AddTwoColumnRow(container, "First name:", person.FirstName));

                if (!string.IsNullOrEmpty(person.LastName))
                    col.Item().Element(container => AddTwoColumnRow(container, "Last name:", person.LastName));

                if (!string.IsNullOrEmpty(person.Nationality))
                    col.Item().Element(container => AddTwoColumnRow(container, "Nationality:", person.Nationality));

                if (person.DateOfBirth != null)
                    col.Item().Element(container => AddTwoColumnRow(container, "Date of birth:", person.DateOfBirth?.ToString("dd-MM-yyyy")));

                if (!string.IsNullOrEmpty(person.PersonType.ToString()))
                {
                    col.Item().Element(container => AddTwoColumnRow(container, "Person type:", GetFriendlyPersonTypeText(person.PersonType)));
                }

                if (!string.IsNullOrEmpty(person.Category.ToString()))
                {
                    col.Item().Element(container => AddTwoColumnRow(container, "Category:", GetFriendlyCategoryText(person.Category)));
                }

                if (!string.IsNullOrEmpty(person.ResidentCountry))
                    col.Item().Element(container => AddTwoColumnRow(container, "Resident country:", person.ResidentCountry));

                var registeredAddress = person.Addresses.FirstOrDefault(a => a.Type == AddressType.Registered);
                var postalAddress = person.Addresses.FirstOrDefault(a => a.Type == AddressType.Postal);

                if (registeredAddress != null)
                {
                    col.Item().Element(container =>
                    {
                        AddTwoColumnRow(container, "Registered address: ",
                            $"Street Address: {registeredAddress.StreetAddress}\n" +
                            $"Locality: {registeredAddress.Locality}\n" +
                            $"Postcode: {registeredAddress.PostalCode}\n" +
                            $"Country: {registeredAddress.CountryName}");
                    });
                }

                if (postalAddress != null)
                {
                    col.Item().Element(container =>
                    {
                        AddTwoColumnRow(container, "Postal address: ",
                            $"Street Address: {postalAddress.StreetAddress}\n" +
                            $"Locality: {postalAddress.Locality}\n" +
                            $"Postcode: {postalAddress.PostalCode}\n" +
                            $"Country: {postalAddress.CountryName}");
                    });
                }

                if (person.ControlConditions.Count != 0)
                {
                    col.Item().Element(container =>
                    {
                        AddTwoColumnRow(container, "Control conditions:", string.Join("\n", person.ControlConditions));
                    });
                }

                if (person.IndividualTrust != null)
                {
                    col.Item().Text("Individual Trust Information:").Bold();
                    if (!string.IsNullOrEmpty(person.IndividualTrust.FirstName))
                        col.Item().Element(container => AddTwoColumnRow(container, "First name:", person.IndividualTrust.FirstName));
                    if (!string.IsNullOrEmpty(person.IndividualTrust.LastName))
                        col.Item().Element(container => AddTwoColumnRow(container, "Last name:", person.IndividualTrust.LastName));
                    if (!string.IsNullOrEmpty(person.IndividualTrust.Nationality))
                        col.Item().Element(container => AddTwoColumnRow(container, "Nationality:", person.IndividualTrust.Nationality));
                    if (person.IndividualTrust.DateOfBirth != null)
                        col.Item().Element(container => AddTwoColumnRow(container, "Date of birth:", person.IndividualTrust.DateOfBirth?.ToString("dd-MM-yyyy")));
                }

                if (person.Organisation != null)
                {
                    col.Item().Text("Organisation Information:").Bold();
                    if (!string.IsNullOrEmpty(person.Organisation.Name))
                        col.Item().Element(container => AddTwoColumnRow(container, "Name:", person.Organisation.Name));
                    if (!string.IsNullOrEmpty(person.Organisation.RegisteredLegalForm))
                        col.Item().Element(container => AddTwoColumnRow(container, "Registered legal form:", person.Organisation.RegisteredLegalForm));
                    if (!string.IsNullOrEmpty(person.Organisation.LawRegistered))
                        col.Item().Element(container => AddTwoColumnRow(container, "Law registered:", person.Organisation.LawRegistered));
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

        return $"{address.StreetAddress}, {address.Locality}, {address.PostalCode}, {address.CountryName}";
    }

    private string GetFriendlyCategoryText(OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType category)
    {
        return category switch
        {
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv => "Person with significant control (Individual)",
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForIndiv => "Director or individual with same responsibilities (Individual)",
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForIndiv => "Other individual with significant influence or control (Individual)",
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust => "Person with significant control (Trust)",
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForTrust => "Director or individual with same responsibilities (Trust)",
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForTrust => "Other individual with significant influence or control (Trust)",
            _ => "Unknown category"
        };
    }

    private string GetFriendlyPersonTypeText(OrganisationInformation.Persistence.ConnectedEntity.ConnectedPersonType personType)
    {
        return personType switch
        {
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedPersonType.Individual => "Individual",
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedPersonType.TrustOrTrustee => "Trust or Trustee",
            _ => "Unknown person type"
        };
    }
}