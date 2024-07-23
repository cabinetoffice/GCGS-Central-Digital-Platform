using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Constants;

public enum ConnectedEntityOrganisationCategoryType
{
    RegisteredCompany = 1,
    DirectorOrTheSameResponsibilities,
    ParentOrSubsidiaryCompany,
    ACompanyYourOrganisationHasTakenOver,
    AnyOtherOrganisationWithSignificantInfluenceOrControl,
}

public static class ConnectedEntityOrganisationCategoryTypeExtensions
{
    public static WebApiClient.ConnectedOrganisationCategory AsApiClientConnectedEntityOrganisationCategoryType(this ConnectedEntityOrganisationCategoryType connectedEntityOrganisationCategoryType)
    {
        return connectedEntityOrganisationCategoryType switch
        {
            ConnectedEntityOrganisationCategoryType.RegisteredCompany => WebApiClient.ConnectedOrganisationCategory.RegisteredCompany,
            ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities => WebApiClient.ConnectedOrganisationCategory.DirectorOrTheSameResponsibilities,
            ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany => WebApiClient.ConnectedOrganisationCategory.ParentOrSubsidiaryCompany,
            ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver => WebApiClient.ConnectedOrganisationCategory.ACompanyYourOrganisationHasTakenOver,
            ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl => WebApiClient.ConnectedOrganisationCategory.AnyOtherOrganisationWithSignificantInfluenceOrControl,
            _ => WebApiClient.ConnectedOrganisationCategory.RegisteredCompany,
        };
    }
    public static ConnectedEntityOrganisationCategoryType AsConnectedEntityOrganisationCategoryType(this WebApiClient.ConnectedOrganisationCategory connectedOrganisationCategory)
    {
        return connectedOrganisationCategory switch
        {
            WebApiClient.ConnectedOrganisationCategory.RegisteredCompany => ConnectedEntityOrganisationCategoryType.RegisteredCompany,
            WebApiClient.ConnectedOrganisationCategory.DirectorOrTheSameResponsibilities => ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities,
            WebApiClient.ConnectedOrganisationCategory.ParentOrSubsidiaryCompany => ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany,
            WebApiClient.ConnectedOrganisationCategory.ACompanyYourOrganisationHasTakenOver => ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver,
            WebApiClient.ConnectedOrganisationCategory.AnyOtherOrganisationWithSignificantInfluenceOrControl => ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl,
            _ => ConnectedEntityOrganisationCategoryType.RegisteredCompany,
        };
    }
    public static string Description(this ConnectedEntityOrganisationCategoryType categoryType, bool registeredWithCompanyHouse)
    {
        var baseDescription = categoryType switch
        {
            ConnectedEntityOrganisationCategoryType.RegisteredCompany => "a registered company",
            ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities => "a director or the same responsibilities",
            ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany => "a parent or subsidiary company",
            ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver => "a company your organisation has taken over",
            ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl => "any other organisation with significant influence or control",
            _ => throw new NotImplementedException()
        };

        return registeredWithCompanyHouse ? baseDescription.Substring(2) : $"equivalent to {baseDescription}";
    }

    public static string Catption(this ConnectedEntityOrganisationCategoryType categoryType, bool registeredWithCompanyHouse)
    {
        var baseCaption = categoryType switch
        {
            ConnectedEntityOrganisationCategoryType.RegisteredCompany => "Registered company",
            ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities => "Director or the same responsibilities",
            ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany => "Parent or subsidiary company",
            ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver => "a company your organisation has taken over",
            ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl => "Other organisation with significant influence or control",
            _ => throw new NotImplementedException()
        };
        return registeredWithCompanyHouse ? baseCaption : $"Equivalent to {baseCaption.ToLower()}";
    }
}