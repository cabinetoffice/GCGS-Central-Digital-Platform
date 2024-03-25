namespace CO.CDP.OrganisationApp.Models;

public class RegistrationDetails
{
    public Guid TenantId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? OrganisationName { get; set; }

        public string? OrganisationType { get; set; }

        public string? OrganisationEmailAddress { get; set; }

        public string? OrganisationTelephoneNumber { get; set; }

        public string? OrganisationIdentificationNumber { get; set; }
    }
}