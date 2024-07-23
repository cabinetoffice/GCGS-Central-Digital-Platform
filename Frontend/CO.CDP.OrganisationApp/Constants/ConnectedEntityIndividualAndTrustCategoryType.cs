using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Constants;

public enum ConnectedEntityIndividualAndTrustCategoryType
{
    PersonWithSignificantControlForIndividual = 1,
    DirectorOrIndividualWithTheSameResponsibilitiesForIndividual,
    AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual,
    PersonWithSignificantControlForTrust,
    DirectorOrIndividualWithTheSameResponsibilitiesForTrust,
    AnyOtherIndividualWithSignificantInfluenceOrControlForTrust
}

public static class ConnectedEntityIndividualAndTrustCategoryTypeExtensions
{
    public static WebApiClient.ConnectedIndividualAndTrustCategory AsApiClientConnectedIndividualAndTrustCategory(this ConnectedEntityIndividualAndTrustCategoryType connectedEntityIndividualAndTrustCategoryType)
    {
        return connectedEntityIndividualAndTrustCategoryType switch
        {
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual => WebApiClient.ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual => WebApiClient.ConnectedIndividualAndTrustCategory.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual => WebApiClient.ConnectedIndividualAndTrustCategory.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust => WebApiClient.ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForIndividual,
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust => WebApiClient.ConnectedIndividualAndTrustCategory.DirectorOrIndividualWithTheSameResponsibilitiesForTrust,
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust => WebApiClient.ConnectedIndividualAndTrustCategory.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust,
            _ => WebApiClient.ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForIndividual
        };
    }
    public static ConnectedEntityIndividualAndTrustCategoryType AsConnectedEntityIndividualAndTrustCategoryType(this WebApiClient.ConnectedIndividualAndTrustCategory connectedIndividualAndTrustCategory)
    {
        return connectedIndividualAndTrustCategory switch
        {
            WebApiClient.ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForIndividual => ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual,
            WebApiClient.ConnectedIndividualAndTrustCategory.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual => ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual,
            WebApiClient.ConnectedIndividualAndTrustCategory.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual => ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual,
            WebApiClient.ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForTrust => ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual,
            WebApiClient.ConnectedIndividualAndTrustCategory.DirectorOrIndividualWithTheSameResponsibilitiesForTrust => ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust,
            WebApiClient.ConnectedIndividualAndTrustCategory.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust => ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust,
            _ => ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual
        };
    }
    public static string Description(this ConnectedEntityIndividualAndTrustCategoryType categoryType, bool registeredWithCompanyHouse)
    {
        var baseDescription = categoryType switch
        {
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual => "person with significant control",
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual => "director or individual with the same responsibilities",
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual => "any other individual with significant influence or control",
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust => "person with significant control",
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust => "director or individual with the same responsibilities",
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust => "any other individual with significant influence or control",
            _ => throw new NotImplementedException()
        };

        return registeredWithCompanyHouse ? baseDescription.Substring(2) : $"equivalent to {baseDescription}";
    }

    public static string Catption(this ConnectedEntityIndividualAndTrustCategoryType categoryType, bool registeredWithCompanyHouse)
    {
        var baseCaption = categoryType switch
        {
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual => "Person with significant control",
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual => "Director or individual with the same responsibilities",
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual => "Other individual with control",
            ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust => "Person with significant control",
            ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust => "Director or individual with the same responsibilities",
            ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust => "Other individual with control",
            _ => throw new NotImplementedException()
        };
        return registeredWithCompanyHouse ? baseCaption : $"Equivalent to {baseCaption.ToLower()}";
    }
}