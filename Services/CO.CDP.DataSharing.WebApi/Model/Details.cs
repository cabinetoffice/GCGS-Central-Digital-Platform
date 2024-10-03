namespace CO.CDP.DataSharing.WebApi.Model;

public class Details
{
    public LegalForm? LegalForm { get; set; }
}

public record LegalForm
{
    public bool RegisteredUnderAct2006 { get; init; }
    public string? RegisteredLegalForm { get; init; }
    public string? LawRegistered { get; init; }
    public string? RegistrationDate { get; init; }
}