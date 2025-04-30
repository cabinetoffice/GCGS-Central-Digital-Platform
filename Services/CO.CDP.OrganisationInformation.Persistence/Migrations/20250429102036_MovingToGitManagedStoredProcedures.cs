using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MovingToGitManagedStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(File.ReadAllText("Migrations/StoredProcedures/get_shared_consent_details.psql"));
            migrationBuilder.Sql(File.ReadAllText("Migrations/StoredProcedures/create_shared_consent_snapshot.psql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(File.ReadAllText("Migrations/StoredProcedures/get_shared_consent_details-PREVIOUS.psql"));
            migrationBuilder.Sql(File.ReadAllText("Migrations/StoredProcedures/create_shared_consent_snapshot-PREVIOUS.psql"));
        }
    }
}
