using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using Address = CO.CDP.OrganisationInformation.Address;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;

namespace CO.CDP.DataSharing.WebApi.DataService;

public class DataService(IShareCodeRepository shareCodeRepository) : IDataService
{
    public async Task<SharedSupplierInformation> GetSharedSupplierInformationAsync(string shareCode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(shareCode)
                            ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);
        return MapToSharedSupplierInformation(sharedConsent);
    }

    private SharedSupplierInformation MapToSharedSupplierInformation(SharedConsent sharedConsent) =>
        new()
        {
            BasicInformation = MapToBasicInformation(sharedConsent.Organisation)
        };

    private BasicInformation MapToBasicInformation(Organisation organisation)
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

        var qualifications = supplierInfo.CompletedQualification
            ? supplierInfo.Qualifications.Select(q => new BasicQualification
            {
                Guid = q.Guid,
                AwardedByPersonOrBodyName = q.AwardedByPersonOrBodyName,
                DateAwarded = q.DateAwarded,
                Name = q.Name
            }).ToList()
            : [];

        var tradeAssurances = supplierInfo.CompletedTradeAssurance
            ? supplierInfo.TradeAssurances.Select(t => new BasicTradeAssurance
            {
                Guid = t.Guid,
                AwardedByPersonOrBodyName = t.AwardedByPersonOrBodyName,
                ReferenceNumber = t.ReferenceNumber,
                DateAwarded = t.DateAwarded
            }).ToList()
            : [];

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
            Qualifications = qualifications,
            TradeAssurances = tradeAssurances,
            OrganisationType = organisationType,
            LegalForm = legalForm
        };
    }
}