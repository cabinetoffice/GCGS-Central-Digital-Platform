using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;

namespace CO.CDP.DataSharing.WebApi.DataService;

public class DataService : IDataService
{
    private readonly IOrganisationRepository _organisationRepository;

    public DataService(IOrganisationRepository organisationRepository)
    {
        _organisationRepository = organisationRepository;
    }

    public async Task<SharedSupplierInformation> GetSharedSupplierInformationAsync(SharedConsent sharedConsent)
    {
        var organisation = await _organisationRepository.Find(sharedConsent.OrganisationId);

        if (organisation?.SupplierInfo == null)
        {
            throw new SupplierInformationNotFoundException("Supplier information not found.");
        }

        var basicInformation = MapToBasicInformation(organisation);
        var sharedSupplierInfo = new SharedSupplierInformation
        {
            BasicInformation = basicInformation
        };
        return sharedSupplierInfo;
    }

    public BasicInformation MapToBasicInformation(Organisation organisation)
    {
        var supplierInfo = organisation.SupplierInfo;

        var registeredAddress = supplierInfo?.CompletedRegAddress == true
            ? organisation.Addresses
                .Where(a => a.Type == AddressType.Registered)
                .Select(a => new CO.CDP.OrganisationInformation.Address
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

        var postalAddress = supplierInfo?.CompletedPostalAddress == true
            ? organisation.Addresses
                .Where(a => a.Type == AddressType.Postal)
                .Select(a => new CO.CDP.OrganisationInformation.Address
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

        var vatNumber = supplierInfo?.CompletedVat == true
            ? organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT")?.IdentifierId
            : null;

        var websiteAddress = supplierInfo?.CompletedWebsiteAddress == true
            ? organisation.ContactPoints.FirstOrDefault()?.Url
            : null;

        var emailAddress = supplierInfo?.CompletedEmailAddress == true
            ? organisation.ContactPoints.FirstOrDefault()?.Email
            : null;

        var qualifications = supplierInfo?.CompletedQualification == true
            ? supplierInfo.Qualifications.Select(q => new BasicQualification
            {
                Guid = q.Guid,
                AwardedByPersonOrBodyName = q.AwardedByPersonOrBodyName,
                DateAwarded = q.DateAwarded,
                Name = q.Name
            }).ToList()
            : new List<BasicQualification>();

        var tradeAssurances = supplierInfo?.CompletedTradeAssurance == true
            ? supplierInfo.TradeAssurances.Select(t => new BasicTradeAssurance
            {
                Guid = t.Guid,
                AwardedByPersonOrBodyName = t.AwardedByPersonOrBodyName,
                ReferenceNumber = t.ReferenceNumber,
                DateAwarded = t.DateAwarded
            }).ToList()
            : new List<BasicTradeAssurance>();

        var legalForm = supplierInfo?.CompletedLegalForm == true
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
            SupplierType = supplierInfo?.SupplierType,
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
