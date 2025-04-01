using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PponIdentifierUrlFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
UPDATE identifiers AS i 
SET uri = 'https://organisation.supplier-information.find-tender.service.gov.uk/organisations/'||o.guid
FROM organisations AS o
WHERE i.organisation_id = o.id AND i.scheme  = 'GB-PPON';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
