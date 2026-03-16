using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillEnableActiveApplicationsForOrganisations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO user_management.organisation_applications
                    (
                        organisation_id,
                        application_id,
                        is_active,
                        enabled_at,
                        enabled_by,
                        is_deleted,
                        created_at,
                        created_by
                    )
                SELECT
                    org.id,
                    app.id,
                    TRUE,
                    NOW(),
                    'migration:backfill-org-app-enable',
                    FALSE,
                    NOW(),
                    'migration:backfill-org-app-enable'
                FROM user_management.organisations org
                CROSS JOIN user_management.applications app
                WHERE org.is_deleted = FALSE
                  AND app.is_active = TRUE
                  AND app.is_deleted = FALSE
                ON CONFLICT (organisation_id, application_id) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentional no-op: backfilled application enablements are not removed on rollback.
        }
    }
}
