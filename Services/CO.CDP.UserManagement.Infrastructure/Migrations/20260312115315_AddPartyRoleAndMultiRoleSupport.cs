using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartyRoleAndMultiRoleSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "allows_multiple_role_assignments",
                schema: "user_management",
                table: "applications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int[]>(
                name: "required_party_roles",
                schema: "user_management",
                table: "application_roles",
                type: "integer[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::integer[]");

            // Buyer = 1, Supplier = 3 (PartyRole enum values)
            migrationBuilder.Sql(
                """
                UPDATE user_management.application_roles
                SET required_party_roles = ARRAY[1]
                WHERE application_id = (SELECT id FROM user_management.applications WHERE client_id = 'financial-viability-risk-assessments')
                  AND name IN ('Assessor (internal)', 'Assessor (external)', 'QA (internal)', 'QA (external)');
                """);

            migrationBuilder.Sql(
                """
                UPDATE user_management.application_roles
                SET required_party_roles = ARRAY[3]
                WHERE application_id = (SELECT id FROM user_management.applications WHERE client_id = 'financial-viability-risk-assessments')
                  AND name IN ('Author and Collaborator (internal)', 'Author and Collaborator (external)');
                """);

            migrationBuilder.Sql(
                """
                UPDATE user_management.applications
                SET allows_multiple_role_assignments = TRUE
                WHERE client_id = 'financial-viability-risk-assessments';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "allows_multiple_role_assignments",
                schema: "user_management",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "required_party_roles",
                schema: "user_management",
                table: "application_roles");
        }
    }
}
