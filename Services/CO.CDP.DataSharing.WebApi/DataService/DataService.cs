using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.Localization;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.AspNetCore.Mvc.Localization;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;
using Address = CO.CDP.OrganisationInformation.Address;
using ConnectedIndividualTrust = CO.CDP.DataSharing.WebApi.Model.ConnectedIndividualTrust;
using ConnectedOrganisation = CO.CDP.DataSharing.WebApi.Model.ConnectedOrganisation;
using FormQuestionType = CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;

namespace CO.CDP.DataSharing.WebApi.DataService;

public class DataService(IShareCodeRepository shareCodeRepository, IConnectedEntityRepository connectedEntityRepository, IHtmlLocalizer<FormsEngineResource> localizer) : IDataService
{
    public async Task<SharedSupplierInformation> GetSharedSupplierInformationAsync(string shareCode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(shareCode)
                            ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);
        var allFormSectionsExceptDeclaractions = sharedConsent.AnswerSets.Where(a =>
            a.Section.Type != OrganisationInformation.Persistence.Forms.FormSectionType.Declaration);
        var connectedEntities = await connectedEntityRepository.FindByOrganisation(sharedConsent.Organisation.Guid);

        return new SharedSupplierInformation
        {
            OrganisationId = sharedConsent.Organisation.Guid,
            BasicInformation = MapToBasicInformation(sharedConsent.Organisation),
            ConnectedPersonInformation = MapToConnectedPersonInformation(connectedEntities),
            FormAnswerSetForPdfs = MapFormAnswerSetsForPdf(allFormSectionsExceptDeclaractions),
            AttachedDocuments = MapAttachedDocuments(sharedConsent),
            AdditionalIdentifiers = MapAdditionalIdentifiersForPdf(sharedConsent.Organisation.Identifiers)
        };
    }

    public IEnumerable<Model.Identifier> MapAdditionalIdentifiersForPdf(IEnumerable<Organisation.Identifier> identifiers)
    {
        return identifiers.Select(identifier => new Model.Identifier
        {
            Id = identifier.IdentifierId,
            Scheme = identifier.Scheme,
            LegalName = identifier.LegalName,
            Uri = identifier.Uri
        }).ToList();
    }

    public IEnumerable<FormAnswerSetForPdf> MapFormAnswerSetsForPdf(
        IEnumerable<OrganisationInformation.Persistence.Forms.FormAnswerSet> answerSets)
    {
        var pdfAnswerSets = new List<FormAnswerSetForPdf>();

        foreach (var answerSet in answerSets)
        {
            var pdfAnswerSet = new FormAnswerSetForPdf
            {
                SectionName = localizer[answerSet.Section.Title].Value,
                SectionType = answerSet.Section.Type,
                QuestionAnswers = []
            };

            pdfAnswerSets.Add(pdfAnswerSet);

            foreach (var answer in answerSet.Answers)
            {
                var title = localizer[answer.Question.Title].Value;
                switch (answer.Question.Type)
                {
                    case FormQuestionType.YesOrNo:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{title}",
                                answer.OptionValue ?? (answer.BoolValue == true ? "Yes" : "Not specified")));
                            break;
                        }
                    case FormQuestionType.Date:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{title}",
                                answer.DateValue?.ToString("dd MMMM yyyy") ?? "Not specified"));
                            break;
                        }
                    case FormQuestionType.Url:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{title}",
                               answer.TextValue ?? "Not specified"));
                            break;
                        }
                    case FormQuestionType.FileUpload:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{title}",
                               answer.TextValue ?? "Not specified"));
                            break;
                        }
                    case FormQuestionType.Text:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{title}:",
                                answer.TextValue ?? "Not specified"));
                            break;
                        }
                    case FormQuestionType.MultiLine:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{title}:",
                                answer.TextValue ?? "Not specified"));
                            break;
                        }
                    case FormQuestionType.GroupedSingleChoice:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{title}:",
                                GetTitleFromValue(answer.OptionValue ?? "Not specified")));
                            break;
                        }
                }

            }
        }

        return pdfAnswerSets;
    }

    public static BasicInformation MapToBasicInformation(Organisation organisation)
    {
        var supplierInfo = organisation.SupplierInfo
            ?? throw new SupplierInformationNotFoundException("Supplier information not found.");

        var registeredAddress = supplierInfo.CompletedRegAddress
            ? organisation.Addresses
                .Where(a => a.Type == AddressType.Registered)
                .Select(a => new Address
                {
                    StreetAddress = a.Address.StreetAddress,
                    Locality = a.Address.Locality,
                    PostalCode = a.Address.PostalCode,
                    CountryName = a.Address.CountryName,
                    Country = a.Address.Country,
                    Type = AddressType.Registered
                })
                .FirstOrDefault()
            : null;

        var postalAddress = supplierInfo.CompletedPostalAddress
            ? organisation.Addresses
                .Where(a => a.Type == AddressType.Postal)
                .Select(a => new Address
                {
                    StreetAddress = a.Address.StreetAddress,
                    Locality = a.Address.Locality,
                    PostalCode = a.Address.PostalCode,
                    CountryName = a.Address.CountryName,
                    Country = a.Address.Country,
                    Type = AddressType.Postal
                })
                .FirstOrDefault()
            : null;

        var vatNumber = supplierInfo.CompletedVat
            ? organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT")?.IdentifierId
            : null;

        var websiteAddress = supplierInfo.CompletedWebsiteAddress
            ? organisation.ContactPoints.FirstOrDefault()?.Url
            : null;

        var emailAddress = supplierInfo.CompletedEmailAddress
            ? organisation.ContactPoints.FirstOrDefault()?.Email
            : null;

        var legalForm = supplierInfo.CompletedLegalForm
            ? new BasicLegalForm
            {
                RegisteredUnderAct2006 = supplierInfo.LegalForm!.RegisteredUnderAct2006,
                RegisteredLegalForm = supplierInfo.LegalForm.RegisteredLegalForm,
                LawRegistered = supplierInfo.LegalForm.LawRegistered,
                RegistrationDate = supplierInfo.LegalForm.RegistrationDate
            }
            : null;

        var organisationType = organisation.Roles.Contains(PartyRole.Tenderer)
            ? OrganisationType.Supplier
            : OrganisationType.Buyer;

        return new BasicInformation
        {
            SupplierType = supplierInfo.SupplierType,
            RegisteredAddress = registeredAddress,
            PostalAddress = postalAddress,
            VatNumber = vatNumber,
            WebsiteAddress = websiteAddress,
            EmailAddress = emailAddress,
            OrganisationType = organisationType,
            LegalForm = legalForm,
            OrganisationName = organisation.Name
        };
    }

    public static List<ConnectedPersonInformation> MapToConnectedPersonInformation(IEnumerable<ConnectedEntity?> entities)
    {
        var connectedPersonList = new List<ConnectedPersonInformation>();

        foreach (var entity in entities)
        {
            if (entity != null)
            {
                var connectedPersonType = entity.IndividualOrTrust?.ConnectedType != null ?
                    entity.IndividualOrTrust.ConnectedType :
                    ConnectedPersonType.Individual;

                var connectedEntityIndividualAndTrustCategoryType = entity.IndividualOrTrust?.Category != null ?
                    entity.IndividualOrTrust.Category :
                    (connectedPersonType == ConnectedPersonType.Individual ?
                        ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv :
                        ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust);

                var connectedOrganisationCategoryType = entity.Organisation?.Category ?? ConnectedOrganisationCategory.RegisteredCompany;

                var individualTrust = entity.IndividualOrTrust != null ? new ConnectedIndividualTrust(
                    entity.IndividualOrTrust.FirstName,
                    entity.IndividualOrTrust.LastName,
                    entity.IndividualOrTrust.DateOfBirth,
                    entity.IndividualOrTrust.Nationality,
                    connectedEntityIndividualAndTrustCategoryType,
                    connectedPersonType,
                    entity.IndividualOrTrust?.ControlCondition.Select(c => c.ToString()).ToList() ?? new List<string>(),
                    entity.IndividualOrTrust?.ResidentCountry
                ) : null;

                var organisation = entity.Organisation != null ? new ConnectedOrganisation(
                    entity.Organisation.Name,
                    entity.Organisation.RegisteredLegalForm,
                    entity.Organisation.LawRegistered,
                    entity.Organisation.ControlCondition.Select(c => c.ToString()).ToList(),
                    entity.Organisation.InsolvencyDate,
                    entity.CompanyHouseNumber,
                    entity.OverseasCompanyNumber,
                    entity.Organisation.OrganisationId
                ) : null;

                var addresses = entity.Addresses.Select(address => new ConnectedAddress(
                    address.Address.StreetAddress,
                    address.Address.Locality,
                    address.Address.Region ?? "",
                    address.Address.PostalCode,
                    address.Address.CountryName,
                    address.Type
                )).ToList();

                connectedPersonList.Add(new ConnectedPersonInformation(
                    entity.Guid,
                    entity.IndividualOrTrust?.FirstName ?? string.Empty,
                    entity.IndividualOrTrust?.LastName ?? string.Empty,
                    entity.IndividualOrTrust?.Nationality,
                    entity.IndividualOrTrust?.DateOfBirth,
                    connectedPersonType,
                    connectedEntityIndividualAndTrustCategoryType,
                    entity.IndividualOrTrust?.ResidentCountry,
                    addresses,
                    entity.IndividualOrTrust?.ControlCondition.Select(c => c.ToString()).ToList() ?? new List<string>(),
                    entity.CompanyHouseNumber,
                    individualTrust,
                    organisation,
                    entity.EntityType,
                    connectedOrganisationCategoryType
                ));
            }
        }

        return connectedPersonList;
    }

    private static List<string> MapAttachedDocuments(OrganisationInformation.Persistence.Forms.SharedConsent sharedConsent)
    {
        var attachedDocuments = new List<string>();
        foreach (var answerSet in sharedConsent.AnswerSets)
        {
            foreach (var answer in answerSet.Answers)
            {
                if (answer.Question.Type == FormQuestionType.FileUpload && !string.IsNullOrWhiteSpace(answer.TextValue))
                {
                    attachedDocuments.Add(answer.TextValue);
                }
            }
        }
        return attachedDocuments;
    }

    private static string GetTitleFromValue(string value)
    {
        return value switch
        {
            "acting_improperly" => "Acting improperly in procurement",
            "breach_of_contract" => "Breach of contract and poor performance",
            "environmental_misconduct" => "Environmental misconduct",
            "infringement_of_competition" => "Infringement of Competition Act 1998, under Chapter II prohibition",
            "insolvency_bankruptcy" => "Insolvency or bankruptcy",
            "labour_market_misconduct" => "Labour market misconduct",
            "competition_law_infringements" => "Potential competition and competition law infringements",
            "professional_misconduct" => "Professional misconduct",
            "substantial_part_business" => "Suspension or ceasing to carry on all or a substantial part of a business",

            "adjustments_for_tax_arrangements" => "Adjustments for tax arrangements that are abusive",
            "defeat_in_respect" => "Defeat in respect of notifiable tax arrangements",
            "failure_to_cooperate" => "Failure to cooperate with an investigation",
            "finding_by_HMRC" => "Finding by HMRC, in exercise of its powers in respect of VAT, of abusive practice",
            "penalties_for_transactions" => "Penalties for transactions connected with VAT fraud and evasion of tax or duty",
            "penalties_payable" => "Penalties payable for errors in tax documentation and failure to notify, and certain VAT and excise",

            "ancillary_offences_aiding" => "Ancillary offences - aiding, abetting, encouraging or assisting crime",
            "cartel_offences" => "Cartel offences",
            "corporate_manslaughter" => "Corporate manslaughter or homicide",
            "labour_market" => "Labour market, slavery and human trafficking offences",
            "organised_crime" => "Organised crime",
            "tax_offences" => "Tax offences",
            "terrorism_and_offences" => "Terrorism and offences having a terrorist connection",
            "theft_fraud" => "Theft, fraud and bribery",

            "Not specified" => "Not specified",
            _ => "Unknown"
        };
    }
}