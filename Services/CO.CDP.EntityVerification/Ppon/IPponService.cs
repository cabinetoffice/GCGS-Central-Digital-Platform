using CO.CDP.OrganisationInformation;

namespace CO.CDP.EntityVerification.Ppon;

public interface IPponService
{
    string GeneratePponId(OrganisationType type);
}