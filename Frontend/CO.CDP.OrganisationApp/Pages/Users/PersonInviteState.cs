namespace CO.CDP.OrganisationApp.Pages.Users;

public class PersonInviteState
{
    public const string TempDataKey = "PersonInviteTempData";
    public Guid? Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<string> Scopes { get; set; }
}