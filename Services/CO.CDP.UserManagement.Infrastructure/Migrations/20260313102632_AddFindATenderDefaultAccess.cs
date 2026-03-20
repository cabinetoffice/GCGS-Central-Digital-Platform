using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFindATenderDefaultAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_enabled_by_default",
                schema: "user_management",
                table: "applications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                INSERT INTO user_management.applications
                    (name, client_id, description, category, is_active, is_enabled_by_default, is_deleted, created_at, created_by)
                VALUES
                    ('Find a Tender', 'find-a-tender', 'View and manage procurement opportunities, supplier information, and notices for public sector buying and bidding.', 'Procurement', TRUE, TRUE, FALSE, NOW(), 'migration:add-find-a-tender-default-access')
                ON CONFLICT (client_id) DO UPDATE
                SET
                    name = EXCLUDED.name,
                    description = EXCLUDED.description,
                    category = EXCLUDED.category,
                    is_active = TRUE,
                    is_enabled_by_default = TRUE,
                    is_deleted = FALSE,
                    deleted_at = NULL,
                    deleted_by = NULL,
                    modified_at = NOW(),
                    modified_by = 'migration:add-find-a-tender-default-access';
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO user_management.organisation_applications
                    (organisation_id, application_id, is_active, enabled_at, enabled_by, is_deleted, created_at, created_by)
                SELECT
                    org.id,
                    app.id,
                    TRUE,
                    NOW(),
                    'migration:add-find-a-tender-default-access',
                    FALSE,
                    NOW(),
                    'migration:add-find-a-tender-default-access'
                FROM user_management.organisations org
                CROSS JOIN user_management.applications app
                WHERE app.client_id = 'find-a-tender'
                ON CONFLICT (organisation_id, application_id) DO UPDATE
                SET
                    is_active = TRUE,
                    enabled_at = NOW(),
                    enabled_by = 'migration:add-find-a-tender-default-access',
                    is_deleted = FALSE,
                    deleted_at = NULL,
                    deleted_by = NULL,
                    modified_at = NOW(),
                    modified_by = 'migration:add-find-a-tender-default-access';
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO user_management.application_roles
                    (application_id, name, description, required_party_roles, is_active, is_deleted, created_at, created_by)
                SELECT app.id, role_data.name, role_data.description, role_data.required_party_roles, TRUE, FALSE, NOW(), 'migration:add-find-a-tender-default-access'
                FROM user_management.applications app
                INNER JOIN (
                    VALUES
                        ('Viewer (buyer)', 'Can view organisation information and supplier information, and manage notices.', ARRAY[1]::integer[]),
                        ('Viewer (supplier)', 'Can view organisation information and supplier information.', ARRAY[3]::integer[]),
                        ('Editor (buyer)', 'Can create share codes; view, add, and edit organisation and supplier information; create API keys to share information with external procurement platforms and authorised integration partners; and manage notices.', ARRAY[1]::integer[]),
                        ('Editor (supplier)', 'Can create share codes, and view, add, and edit organisation and supplier information.', ARRAY[3]::integer[])
                ) AS role_data(name, description, required_party_roles)
                    ON app.client_id = 'find-a-tender'
                ON CONFLICT (application_id, name) DO UPDATE
                SET
                    description = EXCLUDED.description,
                    required_party_roles = EXCLUDED.required_party_roles,
                    is_active = TRUE,
                    is_deleted = FALSE,
                    deleted_at = NULL,
                    deleted_by = NULL,
                    modified_at = NOW(),
                    modified_by = 'migration:add-find-a-tender-default-access';
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM user_management.application_roles ar
                USING user_management.applications app
                WHERE ar.application_id = app.id
                  AND app.client_id = 'find-a-tender'
                  AND ar.name NOT IN
                  (
                    'Viewer (buyer)',
                    'Viewer (supplier)',
                    'Editor (buyer)',
                    'Editor (supplier)'
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_enabled_by_default",
                schema: "user_management",
                table: "applications");

            migrationBuilder.Sql(
                """
                DELETE FROM user_management.application_roles
                WHERE (created_by = 'migration:add-find-a-tender-default-access' OR modified_by = 'migration:add-find-a-tender-default-access')
                  AND name IN
                  (
                    'Viewer (buyer)',
                    'Viewer (supplier)',
                    'Editor (buyer)',
                    'Editor (supplier)'
                  );
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM user_management.organisation_applications oa
                USING user_management.applications app
                WHERE oa.application_id = app.id
                  AND app.client_id = 'find-a-tender'
                  AND oa.created_by = 'migration:add-find-a-tender-default-access';
                """);
        }
    }
}
