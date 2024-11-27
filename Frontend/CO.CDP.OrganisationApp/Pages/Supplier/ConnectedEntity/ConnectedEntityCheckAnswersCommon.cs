using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using ConnectedEntityType = CO.CDP.OrganisationApp.Constants.ConnectedEntityType;

namespace CO.CDP.OrganisationApp.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityCheckAnswersCommon
{
    public static ConnectedEntityState GetConnectedEntityStateFromEntity(Guid organisationId, CO.CDP.Organisation.WebApiClient.ConnectedEntity connectedEntity)
    {
        var connectedEntityType = connectedEntity.EntityType.AsConnectedEntityType();
        var controlConditions = connectedEntityType == ConnectedEntityType.Organisation
            ? connectedEntity.Organisation?.ControlCondition
            : connectedEntity.IndividualOrTrust?.ControlCondition;

        var registerAddress = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Registered);
        var postalAddress = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Postal);


        var state = new ConnectedEntityState()
        {
            CompaniesHouseNumber = connectedEntity.CompanyHouseNumber,
            ConnectedEntityId = connectedEntity.Id,
            ConnectedEntityIndividualAndTrustCategoryType = connectedEntity.IndividualOrTrust?.Category.AsConnectedEntityIndividualAndTrustCategoryType(),
            ConnectedEntityOrganisationCategoryType = connectedEntity.Organisation?.Category.AsConnectedEntityOrganisationCategoryType(),
            ConnectedEntityType = connectedEntityType,
            HasCompaniesHouseNumber = (connectedEntity.CompanyHouseNumber == null ? null : (!string.IsNullOrEmpty(connectedEntity.CompanyHouseNumber))),
            InsolvencyDate = connectedEntity.Organisation?.InsolvencyDate,
            LawRegistered = connectedEntity.Organisation?.LawRegistered,
            LegalForm = connectedEntity.Organisation?.RegisteredLegalForm,
            HasLegalForm = (connectedEntity.Organisation?.RegisteredLegalForm == null ? null : (!string.IsNullOrEmpty(connectedEntity.Organisation?.RegisteredLegalForm))),
            OrganisationName = connectedEntity.Organisation?.Name,
            PostalAddress = (postalAddress != null ? new ConnectedEntityState.Address
            {
                AddressLine1 = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Postal)?.StreetAddress,
                CountryName = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Postal)?.CountryName,
                Country = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Postal)?.Country,
                Postcode = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Postal)?.PostalCode,
                TownOrCity = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Postal)?.Locality
            } : null),
            RegistrationDate = connectedEntity.RegisteredDate,
            HasRegistrationDate = connectedEntity.RegisteredDate.HasValue,
            RegisterName = connectedEntity.RegisterName,
            RegisteredAddress = (registerAddress != null ? new ConnectedEntityState.Address
            {
                AddressLine1 = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Registered)?.StreetAddress,
                CountryName = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Registered)?.CountryName,
                Country = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Registered)?.Country,
                Postcode = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Registered)?.PostalCode,
                TownOrCity = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == CO.CDP.Organisation.WebApiClient.AddressType.Registered)?.Locality
            } : null),
            ControlConditions = ControlConditionCollectionToList(controlConditions),
            Nationality = connectedEntity.IndividualOrTrust?.Nationality,
            SupplierOrganisationId = organisationId,
            FirstName = connectedEntity.IndividualOrTrust?.FirstName,
            LastName = connectedEntity.IndividualOrTrust?.LastName,
            DateOfBirth = connectedEntity.IndividualOrTrust?.DateOfBirth,
            OverseasCompaniesHouseNumber = connectedEntity.OverseasCompanyNumber,
            HasOverseasCompaniesHouseNumber = (connectedEntity.OverseasCompanyNumber == null ? null : (!string.IsNullOrEmpty(connectedEntity.OverseasCompanyNumber))),
            SupplierHasCompanyHouseNumber = connectedEntity.HasCompnayHouseNumber,
            DirectorLocation = connectedEntity.IndividualOrTrust?.ResidentCountry
        };

        return state;
    }

    private static List<ConnectedEntityControlCondition> ControlConditionCollectionToList(ICollection<ControlCondition>? controlConditionCollection)
    {
        List<ConnectedEntityControlCondition> controlConditions = new();

        if (controlConditionCollection == null)
        {
            return controlConditions;
        }

        foreach (var cc in controlConditionCollection)
        {
            var controlConditionCe = cc.AsConnectedEntityClientControlCondition();
            controlConditions.Add(controlConditionCe);
        }

        return controlConditions;
    }

    public static bool SetShowRegisterDate(ConnectedEntityState state)
    {
        if (state.ConnectedEntityType == ConnectedEntityType.Organisation)
        {
            return state.ConnectedEntityOrganisationCategoryType == ConnectedEntityOrganisationCategoryType.RegisteredCompany
                || state.ConnectedEntityOrganisationCategoryType == ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl;
        }
        else
        {
            return state.ConnectedEntityIndividualAndTrustCategoryType == ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual
                || state.ConnectedEntityIndividualAndTrustCategoryType == ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual
                || state.ConnectedEntityIndividualAndTrustCategoryType == ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust
                || state.ConnectedEntityIndividualAndTrustCategoryType == ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust;
        }
    }

    public static bool SetShowLegalForm(ConnectedEntityState state)
    {
        if (state.ConnectedEntityType == ConnectedEntityType.Organisation)
        {
            return state.ConnectedEntityOrganisationCategoryType == ConnectedEntityOrganisationCategoryType.RegisteredCompany
                || state.ConnectedEntityOrganisationCategoryType == ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities
                || state.ConnectedEntityOrganisationCategoryType == ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl;
        }
        else
        {
            return false;
        }
    }
}