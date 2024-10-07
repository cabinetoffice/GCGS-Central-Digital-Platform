namespace CO.CDP.DataSharing.WebApi.Model;

public class Details
{
    public LegalForm? LegalForm { get; set; }
    public string? Scale { get; set; }
    public bool Vcse { get; set; }
    public bool ShelteredWorkshop { get; set; }
    public bool PublicServiceMissionOrganization { get; set; }
}

public record LegalForm
{
    public bool RegisteredUnderAct2006 { get; init; }
    public string? RegisteredLegalForm { get; init; }
    public string? LawRegistered { get; init; }
    public string? RegistrationDate { get; init; }
}