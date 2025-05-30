using CO.CDP.OrganisationInformation.Persistence.StoredProcedures;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReloadStoredProceduresAfterGitMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(StoredProcedureScriptLoader.Load("get_shared_consent_details.psql"));
            migrationBuilder.Sql(StoredProcedureScriptLoader.Load("create_shared_consent_snapshot.psql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(StoredProcedureScriptLoader.Load("get_shared_consent_details-PREVIOUS.psql"));
            migrationBuilder.Sql(StoredProcedureScriptLoader.Load("create_shared_consent_snapshot-PREVIOUS.psql"));
        }
    }
}
