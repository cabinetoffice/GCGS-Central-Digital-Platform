using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAgentRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Map existing Agent role (id 0) to Member (id 1)
                UPDATE user_management.user_organisation_memberships
                SET organisation_role_id = 1
                WHERE organisation_role_id = 0;

                UPDATE user_management.invite_role_mappings
                SET organisation_role_id = 1
                WHERE organisation_role_id = 0;

                -- Set default organisation_role_id to Member (1) instead of Agent (0)
                ALTER TABLE user_management.user_organisation_memberships
                    ALTER COLUMN organisation_role_id SET DEFAULT 1;
                ALTER TABLE user_management.invite_role_mappings
                    ALTER COLUMN organisation_role_id SET DEFAULT 1;

                -- Remove the Agent role from organisation_roles
                DELETE FROM user_management.organisation_roles WHERE id = 0 OR display_name = 'Agent';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Recreate Agent role with id 0 if missing
                INSERT INTO user_management.organisation_roles
                    (id, display_name, description, organisation_information_scopes, sync_to_organisation_information, auto_assign_default_applications, is_deleted, created_at, created_by)
                VALUES
                    (0, 'Agent',  'External user not employed by the organisation (for example: consultants or other third-party representatives).', '[]', TRUE, FALSE, FALSE, NOW(), 'migration:remove-agent-role')
                ON CONFLICT (id) DO NOTHING;

                -- Reset defaults back to 0
                ALTER TABLE user_management.user_organisation_memberships
                    ALTER COLUMN organisation_role_id SET DEFAULT 0;
                ALTER TABLE user_management.invite_role_mappings
                    ALTER COLUMN organisation_role_id SET DEFAULT 0;
            ");
        }
    }
}
