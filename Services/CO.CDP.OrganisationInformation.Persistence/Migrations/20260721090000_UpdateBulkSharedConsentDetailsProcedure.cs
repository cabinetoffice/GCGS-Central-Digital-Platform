using CO.CDP.OrganisationInformation.Persistence.StoredProcedures;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    [DbContext(typeof(OrganisationInformationContext))]
    [Migration("20260721090000_UpdateBulkSharedConsentDetailsProcedure")]
    public class UpdateBulkSharedConsentDetailsProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(StoredProcedureScriptLoader.Load("get_bulk_shared_consent_details.psql"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(StoredProcedureScriptLoader.Load("get_bulk_shared_consent_details-PREVIOUS.psql"));
        }
    }
}
