using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingIdentifiersWithUri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE public.identifiers
                SET uri = 
                  CASE
                    WHEN scheme = 'GB-PPON' THEN '/organisations/' || identifier_id
                    WHEN scheme = 'GB-COH' THEN 'https://find-and-update.company-information.service.gov.uk/company/' || identifier_id
                    WHEN scheme = 'GB-CHC' THEN 'https://register-of-charities.charitycommission.gov.uk/charity-search/-/charity-details/' || identifier_id
                    WHEN scheme = 'GB-SC' THEN 'https://www.oscr.org.uk/about-charities/search-the-register/charity-details?number=' || identifier_id
                    WHEN scheme = 'GB-NIC' THEN 'https://www.charitycommissionni.org.uk/charity-details/?regId=' || identifier_id
                    WHEN scheme = 'GB-NHS' THEN 'https://odsportal.digital.nhs.uk/Organisation/OrganisationDetails?organisationId=' || identifier_id
                    WHEN scheme = 'GB-UKPRN' THEN 'https://www.ukrlp.co.uk'
                    WHEN scheme = 'GB-MPR' THEN 'https://mutuals.fca.org.uk/Search/Society/' || identifier_id
                    WHEN scheme = 'GG-RCE' THEN 'https://portal.guernseyregistry.com/e-commerce/company/' || identifier_id
                    WHEN scheme = 'JE-FSC' THEN 'https://www.jerseyfsc.org/registry/registry-entities/entity/' || identifier_id
                    WHEN scheme = 'IM-CR' THEN 'https://services.gov.im/ded/services/companiesregistry/companysearch.iom'

                    ELSE uri
                  END
                WHERE uri = '' AND identifier_id IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE public.identifiers
                SET uri = ''
                WHERE scheme = 'GB-PPON' AND uri = '/organisations/' || identifier_id;
            ");
        }
    }
}
