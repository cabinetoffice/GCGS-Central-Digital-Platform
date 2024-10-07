using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;
using Address = CO.CDP.OrganisationInformation.Address;
using ConnectedIndividualTrust = CO.CDP.DataSharing.WebApi.Model.ConnectedIndividualTrust;
using ConnectedOrganisation = CO.CDP.DataSharing.WebApi.Model.ConnectedOrganisation;

namespace CO.CDP.DataSharing.WebApi.DataService;

public class DataService(IShareCodeRepository shareCodeRepository, IConnectedEntityRepository connectedEntityRepository) : IDataService
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
            FormAnswerSetForPdfs = MapFormAnswerSetsForPdf(allFormSectionsExceptDeclaractions)
        };
    }

    public static IEnumerable<FormAnswerSetForPdf> MapFormAnswerSetsForPdf(
        IEnumerable<OrganisationInformation.Persistence.Forms.FormAnswerSet> answerSets)
    {
        var pdfAnswerSets = new List<FormAnswerSetForPdf>();

        foreach (var answerSet in answerSets)
        {
            var pdfAnswerSet = new FormAnswerSetForPdf() {
                SectionName = answerSet.Section.Title,
                SectionType = answerSet.Section.Type,
                QuestionAnswers = []
            };

            pdfAnswerSets.Add(pdfAnswerSet);

            foreach (var answer in answerSet.Answers)
            {
                switch (answer.Question.Type)
                {
                    case OrganisationInformation.Persistence.Forms.FormQuestionType.YesOrNo:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{answer.Question.Title}",
                                answer.OptionValue ?? "No"));
                            break;
                        }
                    case OrganisationInformation.Persistence.Forms.FormQuestionType.Date:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{answer.Question.Title}",
                                answer.DateValue?.ToString("dd-MM-yyyy") ?? "Not specified"));
                            break;
                        }
                    case OrganisationInformation.Persistence.Forms.FormQuestionType.Url:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{answer.Question.Title}",
                               answer.TextValue ?? ""));
                            break;
                        }
                    case OrganisationInformation.Persistence.Forms.FormQuestionType.FileUpload:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{answer.Question.Title}",
                               answer.TextValue ?? "No"));
                            break;
                        }
                    case OrganisationInformation.Persistence.Forms.FormQuestionType.Text:
                    case OrganisationInformation.Persistence.Forms.FormQuestionType.MultiLine:
                        {
                            pdfAnswerSet.QuestionAnswers.Add(new Tuple<string, string>($"{answer.Question.Title}:",
                                answer.TextValue ?? ""));
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
                    Region = a.Address.Region,
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
                    Region = a.Address.Region,
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
            LegalForm = legalForm
        };
    }

    public static List<ConnectedPersonInformation> MapToConnectedPersonInformation(IEnumerable<ConnectedEntity?> entities)
    {
        var connectedPersonList = new List<ConnectedPersonInformation>();

        foreach (var entity in entities)
        {
            if (entity != null)
            {
                var individualTrust = entity.IndividualOrTrust != null ? new ConnectedIndividualTrust(
                    entity.IndividualOrTrust.FirstName,
                    entity.IndividualOrTrust.LastName,
                    entity.IndividualOrTrust.DateOfBirth,
                    entity.IndividualOrTrust.Nationality,
                    entity.IndividualOrTrust?.Category != null ? entity.IndividualOrTrust.Category : ConnectedPersonCategory.PersonWithSignificantControl,
                    entity.IndividualOrTrust?.ConnectedType != null ? entity.IndividualOrTrust.ConnectedType : ConnectedPersonType.Individual,
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
                    entity.IndividualOrTrust?.ConnectedType != null ? entity.IndividualOrTrust.ConnectedType : ConnectedPersonType.Individual,
                    entity.IndividualOrTrust?.Category != null ? entity.IndividualOrTrust.Category : ConnectedPersonCategory.PersonWithSignificantControl,
                    entity.IndividualOrTrust?.ResidentCountry,
                    addresses,
                    entity.IndividualOrTrust?.ControlCondition.Select(c => c.ToString()).ToList() ?? new List<string>(),
                    entity.CompanyHouseNumber,
                    individualTrust,
                    organisation
                ));
            }
        }

        return connectedPersonList;
    }
}