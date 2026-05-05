using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixFvraAuthorCollaboratorPartyRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Author and Collaborator roles are for supplier/tenderer orgs.
            // OI maps OrganisationType.Supplier → PartyRole.Tenderer (value 4), so these
            // roles must require ARRAY[4] (Tenderer), not ARRAY[3] (Supplier).
            migrationBuilder.Sql(
                """
                UPDATE user_management.application_roles
                SET required_party_roles = ARRAY[4]
                WHERE application_id = (SELECT id FROM user_management.applications WHERE client_id = 'financial-viability-risk-assessments')
                  AND name IN ('Author and Collaborator (internal)', 'Author and Collaborator (external)');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE user_management.application_roles
                SET required_party_roles = ARRAY[3]
                WHERE application_id = (SELECT id FROM user_management.applications WHERE client_id = 'financial-viability-risk-assessments')
                  AND name IN ('Author and Collaborator (internal)', 'Author and Collaborator (external)');
                """);
        }
    }
}
