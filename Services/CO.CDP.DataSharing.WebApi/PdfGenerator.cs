using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.Localization;
using CO.CDP.OrganisationInformation;
using Microsoft.AspNetCore.Mvc.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CO.CDP.DataSharing.WebApi;

public class PdfGenerator(IHtmlLocalizer<FormsEngineResource> localizer) : IPdfGenerator
{
    public Stream GenerateBasicInformationPdf(List<SharedSupplierInformation> supplierInformations, Dictionary<String, DateTimeOffset?> shareCodes)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("DejaVu Sans"));

                if (supplierInformations.Any())
                {
                    var headerText = (supplierInformations.First().OrganisationType == OrganisationType.InformalConsortium)
                                        ? StaticTextResource.PdfGenerator_ConsortiumInformation_Title
                                        : StaticTextResource.PdfGenerator_SupplierInformation_Title;
                    page.Header().Text(headerText).FontSize(14).Bold().AlignCenter();
                }


                page.Content().Column(col =>
                {
                    foreach (var supplierInformation in supplierInformations)
                    {

                        var basicInformation = supplierInformation.BasicInformation;
                        if (basicInformation != null) AddBasicInformationSection(col, basicInformation, supplierInformation.OrganisationType);

                        if (shareCodes.Any() && supplierInformation.OrganisationType == OrganisationType.InformalConsortium)
                        {
                            AddShareCodesInformationSection(col, shareCodes);
                        }

                        var connectedPersonInformation = supplierInformation.ConnectedPersonInformation;
                        var additionalIdentifiers = supplierInformation.AdditionalIdentifiers;

                        AddAdditionalIdentifiersSection(col, additionalIdentifiers);

                        if(supplierInformation.OrganisationType != OrganisationType.InformalConsortium)
                            AddConnectedPersonInformationSection(col, connectedPersonInformation);

                        AddFormSections(col, supplierInformation.FormAnswerSetForPdfs);

                        if (supplierInformation != supplierInformations.Last())
                        {
                            col.Item().PageBreak();
                        }
                    }
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span(StaticTextResource.PdfGenerator_Footer_GeneratedOn);
                    x.Span(" ");
                    x.Span(DateTime.Now.ToString("dd MMMM yyyy")).Bold();
                });
            });
        });

        var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream;
    }

    private void AddShareCodesInformationSection(ColumnDescriptor col, Dictionary<String, DateTimeOffset?> shareCodes)
    {
        var title = StaticTextResource.PdfGenerator_ConsortiumSharecodeInformation_Title;

        col.Item().Text(title).Bold().FontSize(12);
        col.Item().PaddingBottom(10);
        col.Item().LineHorizontal(1);

        var i = 1;
        var submittedAt = string.Empty;

        foreach (var item in shareCodes)
        {
            submittedAt = string.Empty;
            if (item.Value != null && item.Value.HasValue)
            {
                submittedAt = item.Value.Value.ToString("dd MMMM yyyy");
            }

            col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConsortiumSharecodeInformation_Organisation + i, item.Key.ToString()));
            col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConsortiumSharecodeInformation_Sharecode, submittedAt));
            col.Item().PaddingBottom(10);
            i++;
        }

        col.Item().PaddingBottom(10);
        col.Item().LineHorizontal(1);
        col.Item().PaddingBottom(10);
    }

    private void AddFormSections(ColumnDescriptor col, IEnumerable<FormAnswerSetForPdf> formAnswerSet)
    {
        var groupedSections = formAnswerSet.GroupBy(s => s.SectionName);

        foreach (var group in groupedSections)
        {
            col.Item().Text(localizer[group.Key].Value).Bold().FontSize(12);
            col.Item().PaddingBottom(10);
            col.Item().LineHorizontal(1);


            foreach (var answerSet in group)
            {
                if (answerSet.QuestionAnswers != null)
                {
                    foreach (var qa in answerSet.QuestionAnswers)
                    {
                        col.Item().Element(container => AddTwoColumnRow(container, localizer[qa.Item1].Value, qa.Item2));
                    }
                    col.Item().PaddingBottom(10);
                }
            }

            col.Item().LineHorizontal(1);
            col.Item().PaddingBottom(10);
        }
    }

    private void AddBasicInformationSection(ColumnDescriptor col, BasicInformation basicInformation, OrganisationType organisationType)
    {

        var organisationName = StaticTextResource.PdfGenerator_BasicInformation_OrganisationName;
        var title = StaticTextResource.PdfGenerator_OrganisationDetails_Title;

        if (organisationType == OrganisationType.InformalConsortium)
        {
            organisationName = StaticTextResource.PdfGenerator_BasicInformation_ConsortiumName;
            title = StaticTextResource.PdfGenerator_ConsortiumDetails_Title;
        }

        col.Item().Text(title).Bold().FontSize(12);
        col.Item().PaddingBottom(10);

        if (!string.IsNullOrEmpty(basicInformation.OrganisationName?.ToString()))
            col.Item().Element(container => AddTwoColumnRow(container, organisationName, basicInformation.OrganisationName?.ToString()));

        if (!string.IsNullOrEmpty(basicInformation.SupplierType?.ToString()))
            col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_BasicInformation_Suppliertype, basicInformation.SupplierType?.ToString()));

        if (basicInformation.RegisteredAddress != null)
            col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_BasicInformation_RegisteredAddress, FormatAddress(basicInformation.RegisteredAddress)));

        if (basicInformation.PostalAddress != null)
            col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_BasicInformation_PostalAddress, FormatAddress(basicInformation.PostalAddress)));

        if (!string.IsNullOrEmpty(basicInformation.VatNumber))
            col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_BasicInformation_VATNumber, basicInformation.VatNumber));

        if (!string.IsNullOrEmpty(basicInformation.WebsiteAddress))
            col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_BasicInformation_WebsiteAddress, basicInformation.WebsiteAddress));

        if (!string.IsNullOrEmpty(basicInformation.EmailAddress))
            col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_BasicInformation_EmailAddress, basicInformation.EmailAddress));

        if (!string.IsNullOrEmpty(basicInformation.Role))
            col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_BasicInformation_OrganisationType, basicInformation.Role));

        if (basicInformation.OperationTypes.Count != 0)
        {
            col.Item().Element(container =>
            {
                var supplierOperationTypeText = string.Join(", ", basicInformation.OperationTypes.Select(ot =>
                {
                    return GetFriendlySupplierOperationTypeText(ot);                    
                }));

                AddTwoColumnRow(container, StaticTextResource.PdfGenerator_Supplier_OperationType, supplierOperationTypeText);
            });
        }

        if (basicInformation.LegalForm != null)
        {
            col.Item().Element(container =>
            {
                container.Column(column =>
                {
                    column.Item().Element(item => AddTwoColumnRow(item, StaticTextResource.PdfGenerator_BasicInformation_RegisteredLegalForm, basicInformation.LegalForm.RegisteredLegalForm));
                    column.Item().Element(item => AddTwoColumnRow(item, StaticTextResource.PdfGenerator_BasicInformation_LawRegistered, basicInformation.LegalForm.LawRegistered));
                    column.Item().Element(item => AddTwoColumnRow(item, StaticTextResource.PdfGenerator_BasicInformation_RegistrationDate, $"{basicInformation.LegalForm.RegistrationDate:dd MMMM yyyy}"));
                });
            });
        }
        col.Item().PaddingBottom(10);
        col.Item().LineHorizontal(1);
        col.Item().PaddingBottom(10);
    }

    private void AddConnectedPersonInformationSection(ColumnDescriptor col, List<ConnectedEntityInformation> connectedPersons)
    {
        col.Item().Text(StaticTextResource.PdfGenerator_ConnectedPersonInformation_Title).Bold().FontSize(12);
        col.Item().PaddingBottom(10);

        if (connectedPersons.Any())
        {
            foreach (var person in connectedPersons)
            {
                col.Item().LineHorizontal(1);

                if (person.Organisation != null)
                {
                    col.Item().Text($"{StaticTextResource.PdfGenerator_ConnectedOrganisationInformation_Title}:").Bold();
                    col.Item().PaddingBottom(10);
                    if (!string.IsNullOrEmpty(person.Organisation.Name))
                        col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_OrganisationName, person.Organisation.Name));
                    if (!string.IsNullOrEmpty(person.Organisation.RegisteredLegalForm))
                        col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_RegisteredLegalForm, person.Organisation.RegisteredLegalForm));
                    if (!string.IsNullOrEmpty(person.Organisation.LawRegistered))
                        col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_LawRegistered, person.Organisation.LawRegistered));
                }

                if (!string.IsNullOrEmpty(person.FirstName))
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_FirstName, person.FirstName));

                if (!string.IsNullOrEmpty(person.LastName))
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_LastName, person.LastName));

                if (!string.IsNullOrEmpty(person.Nationality))
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_Nationality, person.Nationality));

                if (person.DateOfBirth != null)
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_DateOfBirth, person.DateOfBirth?.ToString("dd MMMM yyyy")));

                if (!string.IsNullOrEmpty(person.CompanyHouseNumber))
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_CompanyHouseNumber, person.CompanyHouseNumber));

                if (!string.IsNullOrEmpty(person.PersonType.ToString()))
                {
                    if (!string.IsNullOrEmpty(person.Organisation?.Name))
                    {
                        col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_OrganisationEntityType, GetFriendlyEntityTypeText(person.EntityType)));
                    }
                    else
                    {
                        col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_Type, GetFriendlyPersonTypeText(person.PersonType)));
                    }
                }

                if (!string.IsNullOrEmpty(person.Organisation?.Name))
                {
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_OrganisationCategory, GetFriendlyOrganisationCategoryText(person.ConnectedOrganisationCategoryType)));
                }
                else if (!string.IsNullOrEmpty(person.ToString()))
                {
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_Category, GetFriendlyCategoryText(person.Category)));
                }

                if (!string.IsNullOrEmpty(person.ResidentCountry))
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_ResidentCountry, person.ResidentCountry));

                var registeredAddress = person.Addresses.FirstOrDefault(a => a.Type == AddressType.Registered);
                var postalAddress = person.Addresses.FirstOrDefault(a => a.Type == AddressType.Postal);

                if (registeredAddress != null)
                {
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_RegisteredAddress,
                        FormatAddress(mapAddressDetails(registeredAddress))));
                }

                if (postalAddress != null)
                {
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_PostalAddress,
                        FormatAddress(mapAddressDetails(postalAddress))));
                }

                if (person.RegistrationDate != null)
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedOrganisationInformation_RegistrationDate, person.RegistrationDate?.ToString("dd MMMM yyyy")));

                if (person.Organisation != null)
                {
                    if (person.Organisation.ControlConditions.Count != 0)
                    {
                        col.Item().Element(container =>
                        {
                            var controlConditionsText = string.Join(", ", person.Organisation.ControlConditions.Select(condition =>
                            {
                                if (Enum.TryParse(condition, out ControlCondition parsedCondition))
                                {
                                    return GetFriendlyControlConditionTypeText(parsedCondition);
                                }
                                return condition;
                            }));

                            AddTwoColumnRow(container, StaticTextResource.PdfGenerator_Organisation_ControlConditions, controlConditionsText);
                        });
                    }
                }

                if (person.ControlConditions.Count != 0)
                {
                    col.Item().Element(container =>
                    {
                        var controlConditionsText = string.Join(", ", person.ControlConditions.Select(condition =>
                        {
                            if (Enum.TryParse(condition, out ControlCondition parsedCondition))
                            {
                                return GetFriendlyControlConditionTypeText(parsedCondition);
                            }
                            return condition;
                        }));

                        AddTwoColumnRow(container, StaticTextResource.PdfGenerator_Organisation_ControlConditions, controlConditionsText);
                    });
                }

                if (person.IndividualTrust != null)
                {
                    col.Item().Text($"{StaticTextResource.PdfGenerator_IndividualTrustInformation_Title}:").Bold();
                    if (!string.IsNullOrEmpty(person.IndividualTrust.FirstName))
                        col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_FirstName, person.IndividualTrust.FirstName));
                    if (!string.IsNullOrEmpty(person.IndividualTrust.LastName))
                        col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_LastName, person.IndividualTrust.LastName));
                    if (!string.IsNullOrEmpty(person.IndividualTrust.Nationality))
                        col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_Nationality, person.IndividualTrust.Nationality));
                    if (person.IndividualTrust.DateOfBirth != null)
                        col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_ConnectedPerson_DateOfBirth, person.IndividualTrust.DateOfBirth?.ToString("dd MMMM yyyy")));
                }

                if (person.IndividualTrust != null)
                {
                    if (person.IndividualTrust.ControlConditions.Count != 0)
                    {
                        col.Item().Element(container =>
                        {
                            var controlConditionsText = string.Join(", ", person.IndividualTrust.ControlConditions.Select(condition =>
                            {
                                if (Enum.TryParse(condition, out ControlCondition parsedCondition))
                                {
                                    return GetFriendlyControlConditionTypeText(parsedCondition);
                                }
                                return condition;
                            }));

                            AddTwoColumnRow(container, StaticTextResource.PdfGenerator_Organisation_ControlConditions, controlConditionsText);
                        });
                    }
                }
                col.Item().PaddingBottom(10);
            }
        }
        else
        {
            col.Item().Text(StaticTextResource.PdfGenerator_NoConnectedPersonInformationAvailableMsg);
        }

        col.Item().LineHorizontal(1);
        col.Item().PaddingBottom(10);
    }

    private Address? mapAddressDetails(ConnectedAddress? address)
    {
        if (address != null)
        {
            return new Address
            {
                StreetAddress = address.StreetAddress,
                Region = address.Region,
                PostalCode = address.PostalCode,
                CountryName = address.CountryName,
                Country = address.CountryName,
                Locality = address.Locality,
                Type = address.Type == AddressType.Registered ? AddressType.Registered : AddressType.Postal
            };
        }
        return null;
    }

    private void AddAdditionalIdentifiersSection(ColumnDescriptor col, IEnumerable<Model.Identifier> additionalIdentifiers)
    {
        col.Item().Text(StaticTextResource.PdfGenerator_AdditionalIdentifiers).Bold().FontSize(12);
        col.Item().PaddingBottom(10);

        if (additionalIdentifiers.Any())
        {
            foreach (var identifier in additionalIdentifiers.Where(i => i.Scheme != "VAT"))
            {
                col.Item().LineHorizontal(1);

                if (!string.IsNullOrEmpty(identifier.Scheme))
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_Scheme, GetOrganisationSchemeTitle(identifier.Scheme)));

                if (!string.IsNullOrEmpty(identifier.Id))
                    col.Item().Element(container => AddTwoColumnRow(container, StaticTextResource.PdfGenerator_IdentifierId, identifier.Id));
                col.Item().PaddingBottom(10);
            }
        }
        else
        {
            col.Item().Text(StaticTextResource.PdfGenerator_NoAdditionalIdentifiersAvailableMsg);
        }
        col.Item().LineHorizontal(1);
        col.Item().PaddingBottom(10);
    }

    private void AddTwoColumnRow(IContainer container, string label, string? value)
    {
        if ((label.Contains('?') && label.Contains(':')) || label.Contains(':'))
        {
            label = label.TrimEnd(':');
        }

        container.Row(row =>
        {
            row.RelativeItem().Text(label).Bold();
            row.RelativeItem().Text(value ?? StaticTextResource.Global_NotApplicable);
        });
    }

    private string FormatAddress(Address? address)
    {
        if (address == null)
            return StaticTextResource.Global_NotApplicable;

        var country = address.CountryName == "United Kingdom" ? "UK" : address.CountryName;
        return $"{address.StreetAddress}, {address.Locality}, {address.PostalCode}, {country}";
    }

    private string GetFriendlyCategoryText(OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType category)
    {
        return category switch
        {
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv => StaticTextResource.PdfGenerator_ConnectedEntityIndividualAndTrustCategory_PersonWithSignificantControl,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForIndiv => StaticTextResource.PdfGenerator_ConnectedEntityIndividualAndTrustCategory_DirectorOrIndividualSameResponsibilities,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForIndiv => StaticTextResource.PdfGenerator_ConnectedEntityIndividualAndTrustCategory_OtherIndividualWithSignificantInfluence,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust => StaticTextResource.PdfGenerator_ConnectedEntityIndividualAndTrustCategory_PersonWithSignificantControl,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForTrust => StaticTextResource.PdfGenerator_ConnectedEntityIndividualAndTrustCategory_DirectorOrIndividualSameResponsibilities,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForTrust => StaticTextResource.PdfGenerator_ConnectedEntityIndividualAndTrustCategory_OtherIndividualWithSignificantInfluence,
            _ => StaticTextResource.PdfGenerator_UnknownCategory
        };
    }

    private string GetFriendlyOrganisationCategoryText(OrganisationInformation.Persistence.ConnectedEntity.ConnectedOrganisationCategory organisationCategory)
    {
        return organisationCategory switch
        {
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedOrganisationCategory.RegisteredCompany => StaticTextResource.PdfGenerator_ConnectedOrganisationCategory_RegisteredCompany,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedOrganisationCategory.DirectorOrTheSameResponsibilities => StaticTextResource.PdfGenerator_ConnectedOrganisationCategory_DirectorSameResponsibilities,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedOrganisationCategory.ParentOrSubsidiaryCompany => StaticTextResource.PdfGenerator_ConnectedOrganisationCategory_ParentSubsidiaryCompany,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedOrganisationCategory.ACompanyYourOrganisationHasTakenOver => StaticTextResource.PdfGenerator_ConnectedOrganisationCategory_TakenOverCompany,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedOrganisationCategory.AnyOtherOrganisationWithSignificantInfluenceOrControl => StaticTextResource.PdfGenerator_ConnectedOrganisationCategory_OtherOrganisationWithInfluence,
            _ => StaticTextResource.PdfGenerator_UnknownCategory
        };
    }

    private string GetFriendlyEntityTypeText(ConnectedEntityType entityType)
    {
        return entityType switch
        {
            ConnectedEntityType.Organisation => StaticTextResource.PdfGenerator_ConnectedEntityType_Organisation,
            ConnectedEntityType.Individual => StaticTextResource.PdfGenerator_ConnectedEntityType_Individual,
            ConnectedEntityType.TrustOrTrustee => StaticTextResource.PdfGenerator_ConnectedEntityType_TrustTrustee,
            _ => StaticTextResource.PdfGenerator_UnknownCategory
        };
    }

    private string GetFriendlyPersonTypeText(OrganisationInformation.Persistence.ConnectedEntity.ConnectedPersonType personType)
    {
        return personType switch
        {
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedPersonType.Individual => StaticTextResource.PdfGenerator_ConnectedPersonType_Individual,
            OrganisationInformation.Persistence.ConnectedEntity.ConnectedPersonType.TrustOrTrustee => StaticTextResource.PdfGenerator_ConnectedPersonType_TrustTrustee,
            _ => StaticTextResource.PdfGenerator_UnknownPersonType
        };
    }

    private string GetFriendlyControlConditionTypeText(ControlCondition controlConditionType)
    {
        return controlConditionType switch
        {
            ControlCondition.OwnsShares => StaticTextResource.PdfGenerator_ControlCondition_OwnsShares,
            ControlCondition.None => StaticTextResource.PdfGenerator_ControlCondition_None,
            ControlCondition.HasVotingRights => StaticTextResource.PdfGenerator_ControlCondition_HasVotingRights,
            ControlCondition.CanAppointOrRemoveDirectors => StaticTextResource.PdfGenerator_ControlCondition_CanAppointRemoveDirectors,
            ControlCondition.HasOtherSignificantInfluenceOrControl => StaticTextResource.PdfGenerator_ControlCondition_HasOtherInfluence,
            _ => StaticTextResource.PdfGenerator_UnknownControlCondition
        };
    }

    private string GetFriendlySupplierOperationTypeText(OperationType operationType)
    {
        return operationType switch
        {
            OperationType.SmallOrMediumSized => StaticTextResource.PdfGenerator_Supplier_OperationType_SmallOrMediumSized_Short,
            OperationType.NonGovernmental => StaticTextResource.PdfGenerator_Supplier_OperationType_NonGovernmental_Short,
            OperationType.SupportedEmploymentProvider => StaticTextResource.PdfGenerator_Supplier_OperationType_SupportedEmploymentProvider_Short,
            OperationType.PublicService => StaticTextResource.PdfGenerator_Supplier_OperationType_PublicService_Short,
            OperationType.None => StaticTextResource.PdfGenerator_Supplier_OperationType_None_Short,
            _ => StaticTextResource.PdfGenerator_Supplier_OperationType_Unknown_Short
        };
    }
    private static string GetOrganisationSchemeTitle(string value)
    {
        return value.ToUpper() switch
        {
            "GB-COH" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_CompaniesHouseNumber,
            "GB-CHC" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_CharityCommission,
            "GB-SC" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_ScottishCharityRegulator,
            "GB-NIC" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_NorthernIrelandCharityCommission,
            "GB-MPR" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_MutualsPublicRegister,
            "GG-RCE" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_GuernseyRegistry,
            "JE-FSC" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_JerseyFinancialServices,
            "IM-CR" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_IsleOfManCompaniesRegistry,
            "GB-NHS" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_NHSRegistry,
            "GB-UKPRN" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_LearningProviderRegistry,
            "VAT" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_VAT,
            "GB-PPON" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_Ppon,
            "OTHER" => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_Other,
            _ => StaticTextResource.PdfGenerator_OrganisationSchemeTitle_Unknown
        };
    }
}