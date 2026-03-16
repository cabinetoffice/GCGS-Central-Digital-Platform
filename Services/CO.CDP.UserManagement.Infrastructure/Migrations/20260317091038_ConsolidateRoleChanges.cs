using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateRoleChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE user_management.user_organisation_memberships
                    ADD COLUMN IF NOT EXISTS organisation_role_id integer NOT NULL DEFAULT 0;
                ALTER TABLE user_management.invite_role_mappings
                    ADD COLUMN IF NOT EXISTS organisation_role_id integer NOT NULL DEFAULT 0;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE user_management.applications
                    ADD COLUMN IF NOT EXISTS is_enabled_by_default boolean NOT NULL DEFAULT false;
            ");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'user_management'
                          AND table_name = 'application_roles'
                          AND column_name = 'required_party_roles'
                          AND data_type = 'ARRAY'
                          AND udt_name = '_text'
                    ) THEN
                        ALTER TABLE user_management.application_roles
                            ALTER COLUMN required_party_roles TYPE integer[]
                            USING required_party_roles::integer[];
                    END IF;
                END $$;

                ALTER TABLE user_management.application_roles
                    ADD COLUMN IF NOT EXISTS organisation_information_scopes text NOT NULL DEFAULT '[]';
                ALTER TABLE user_management.application_roles
                    ADD COLUMN IF NOT EXISTS sync_to_organisation_information boolean NOT NULL DEFAULT false;
            ");

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS user_management.organisation_roles (
                    id integer NOT NULL,
                    display_name character varying(255) NOT NULL,
                    description character varying(1000),
                    sync_to_organisation_information boolean NOT NULL,
                    auto_assign_default_applications boolean NOT NULL,
                    is_deleted boolean NOT NULL,
                    deleted_at timestamp with time zone,
                    deleted_by character varying(255),
                    created_at timestamp with time zone NOT NULL,
                    created_by character varying(255) NOT NULL,
                    modified_at timestamp with time zone,
                    modified_by character varying(255),
                    organisation_information_scopes text NOT NULL DEFAULT '[]',
                    CONSTRAINT "PK_organisation_roles" PRIMARY KEY (id)
                );

                ALTER TABLE user_management.organisation_roles
                    ADD COLUMN IF NOT EXISTS organisation_information_scopes text NOT NULL DEFAULT '[]';
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO user_management.organisation_roles
                    (id, display_name, description, organisation_information_scopes, sync_to_organisation_information, auto_assign_default_applications, is_deleted, created_at, created_by)
                VALUES
                    (0, 'Agent',  'External user not employed by the organisation (for example: consultants or other third-party representatives).', '[]',         TRUE, FALSE, FALSE, NOW(), 'migration:consolidate-role-changes'),
                    (1, 'Member', 'Can access assigned applications only. Cannot manage organisation settings or other users.',                                       '["VIEWER"]', TRUE, TRUE,  FALSE, NOW(), 'migration:consolidate-role-changes'),
                    (2, 'Admin',  'Can add and remove users, enable applications for users, and assign users to applications.',           '["ADMIN"]',  TRUE, TRUE,  FALSE, NOW(), 'migration:consolidate-role-changes'),
                    (3, 'Owner',  'Full control of the organisation. An organisation must have at least one owner.', '["ADMIN"]', TRUE, TRUE, FALSE, NOW(), 'migration:consolidate-role-changes')
                ON CONFLICT (id) DO NOTHING;

                UPDATE user_management.user_organisation_memberships
                SET organisation_role_id =
                    CASE organisation_role
                        WHEN 'Agent'  THEN 0
                        WHEN 'Member' THEN 1
                        WHEN 'Admin'  THEN 2
                        WHEN 'Owner'  THEN 3
                    END
                WHERE organisation_role IS NOT NULL AND organisation_role != '';

                UPDATE user_management.invite_role_mappings
                SET organisation_role_id =
                    CASE organisation_role
                        WHEN 'Agent'  THEN 0
                        WHEN 'Member' THEN 1
                        WHEN 'Admin'  THEN 2
                        WHEN 'Owner'  THEN 3
                    END
                WHERE organisation_role IS NOT NULL AND organisation_role != '';

                UPDATE user_management.application_roles
                SET organisation_information_scopes =
                    CASE
                        WHEN name IN ('Admin', 'Editor', 'Authoriser') THEN '["ADMIN"]'
                        ELSE '[]'
                    END;

                UPDATE user_management.application_roles
                SET sync_to_organisation_information = TRUE
                WHERE organisation_information_scopes != '[]';
                """);

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_user_organisation_memberships_organisation_role_id""
                    ON user_management.user_organisation_memberships (organisation_role_id);
                CREATE INDEX IF NOT EXISTS ""IX_invite_role_mappings_organisation_role_id""
                    ON user_management.invite_role_mappings (organisation_role_id);

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_invite_role_mappings_organisation_roles_organisation_role_id'
                          AND table_schema = 'user_management'
                    ) THEN
                        ALTER TABLE user_management.invite_role_mappings
                            ADD CONSTRAINT ""FK_invite_role_mappings_organisation_roles_organisation_role_id""
                            FOREIGN KEY (organisation_role_id)
                            REFERENCES user_management.organisation_roles (id)
                            ON DELETE RESTRICT;
                    END IF;
                END $$;

                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_user_organisation_memberships_organisation_roles_organisati~'
                          AND table_schema = 'user_management'
                    ) THEN
                        ALTER TABLE user_management.user_organisation_memberships
                            ADD CONSTRAINT ""FK_user_organisation_memberships_organisation_roles_organisati~""
                            FOREIGN KEY (organisation_role_id)
                            REFERENCES user_management.organisation_roles (id)
                            ON DELETE RESTRICT;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE user_management.user_organisation_memberships
                    DROP COLUMN IF EXISTS organisation_role;
                ALTER TABLE user_management.invite_role_mappings
                    DROP COLUMN IF EXISTS organisation_role;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invite_role_mappings_organisation_roles_organisation_role_id",
                schema: "user_management",
                table: "invite_role_mappings");

            migrationBuilder.DropForeignKey(
                name: "FK_user_organisation_memberships_organisation_roles_organisati~",
                schema: "user_management",
                table: "user_organisation_memberships");

            migrationBuilder.DropTable(
                name: "organisation_roles",
                schema: "user_management");

            migrationBuilder.DropIndex(
                name: "IX_user_organisation_memberships_organisation_role_id",
                schema: "user_management",
                table: "user_organisation_memberships");

            migrationBuilder.DropIndex(
                name: "IX_invite_role_mappings_organisation_role_id",
                schema: "user_management",
                table: "invite_role_mappings");

            migrationBuilder.DropColumn(
                name: "organisation_role_id",
                schema: "user_management",
                table: "user_organisation_memberships");

            migrationBuilder.DropColumn(
                name: "organisation_role_id",
                schema: "user_management",
                table: "invite_role_mappings");

            migrationBuilder.DropColumn(
                name: "is_enabled_by_default",
                schema: "user_management",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "organisation_information_scopes",
                schema: "user_management",
                table: "application_roles");

            migrationBuilder.DropColumn(
                name: "sync_to_organisation_information",
                schema: "user_management",
                table: "application_roles");

            migrationBuilder.AddColumn<string>(
                name: "organisation_role",
                schema: "user_management",
                table: "user_organisation_memberships",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "organisation_role",
                schema: "user_management",
                table: "invite_role_mappings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<List<string>>(
                name: "required_party_roles",
                schema: "user_management",
                table: "application_roles",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(int[]),
                oldType: "integer[]");
        }
    }
}
