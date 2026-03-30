using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrectApplicationRoleOiScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- Payments roles should not sync to Organisation Information.
                UPDATE user_management.application_roles ar
                SET organisation_information_scopes = '[]',
                    sync_to_organisation_information = FALSE
                FROM user_management.applications app
                WHERE ar.application_id = app.id
                  AND app.client_id = 'payments';

                -- Find a Tender roles: assign correct OI scopes per role.
                UPDATE user_management.application_roles ar
                SET organisation_information_scopes =
                        CASE ar.name
                            WHEN 'Editor (buyer)'    THEN '["ADMIN", "RESPONDER"]'
                            WHEN 'Editor (supplier)' THEN '["EDITOR", "RESPONDER"]'
                            WHEN 'Viewer (buyer)'    THEN '["VIEWER", "RESPONDER"]'
                            WHEN 'Viewer (supplier)' THEN '["VIEWER", "RESPONDER"]'
                            ELSE '[]'
                        END,
                    sync_to_organisation_information =
                        CASE ar.name
                            WHEN 'Editor (buyer)'    THEN TRUE
                            WHEN 'Editor (supplier)' THEN TRUE
                            WHEN 'Viewer (buyer)'    THEN TRUE
                            WHEN 'Viewer (supplier)' THEN TRUE
                            ELSE FALSE
                        END
                FROM user_management.applications app
                WHERE ar.application_id = app.id
                  AND app.client_id = 'find-a-tender';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- Restore Payments roles to the state ConsolidateRoleChanges left them in
                -- (Admin/Editor/Authoriser were given ["ADMIN"] by that migration).
                UPDATE user_management.application_roles ar
                SET organisation_information_scopes =
                        CASE ar.name
                            WHEN 'Admin'       THEN '["ADMIN"]'
                            WHEN 'Editor'      THEN '["ADMIN"]'
                            WHEN 'Authoriser'  THEN '["ADMIN"]'
                            ELSE '[]'
                        END,
                    sync_to_organisation_information =
                        CASE ar.name
                            WHEN 'Admin'       THEN TRUE
                            WHEN 'Editor'      THEN TRUE
                            WHEN 'Authoriser'  THEN TRUE
                            ELSE FALSE
                        END
                FROM user_management.applications app
                WHERE ar.application_id = app.id
                  AND app.client_id = 'payments';

                -- Restore Find a Tender roles to '[]' / sync=false.
                UPDATE user_management.application_roles ar
                SET organisation_information_scopes = '[]',
                    sync_to_organisation_information = FALSE
                FROM user_management.applications app
                WHERE ar.application_id = app.id
                  AND app.client_id = 'find-a-tender';
                """);
        }
    }
}
