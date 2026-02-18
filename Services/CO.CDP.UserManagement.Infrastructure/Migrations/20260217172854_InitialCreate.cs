using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user_management");

            migrationBuilder.CreateTable(
                name: "applications",
                schema: "user_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    client_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organisations",
                schema: "user_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cdp_organisation_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organisations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "application_permissions",
                schema: "user_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_application_permissions_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "user_management",
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_roles",
                schema: "user_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_application_roles_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "user_management",
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invite_role_mappings",
                schema: "user_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cdp_person_invite_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_role = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invite_role_mappings", x => x.id);
                    table.ForeignKey(
                        name: "FK_invite_role_mappings_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "user_management",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organisation_applications",
                schema: "user_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    enabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    enabled_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    disabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disabled_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organisation_applications", x => x.id);
                    table.ForeignKey(
                        name: "FK_organisation_applications_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "user_management",
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organisation_applications_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "user_management",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_organisation_memberships",
                schema: "user_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_principal_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    cdp_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_role = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    invited_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_organisation_memberships", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_organisation_memberships_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "user_management",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_role_permissions",
                schema: "user_management",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    permission_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "FK_application_role_permissions_application_permissions_permis~",
                        column: x => x.permission_id,
                        principalSchema: "user_management",
                        principalTable: "application_permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_role_permissions_application_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "user_management",
                        principalTable: "application_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invite_role_application_assignments",
                schema: "user_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invite_role_mapping_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_application_id = table.Column<int>(type: "integer", nullable: false),
                    application_role_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invite_role_application_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_invite_role_application_assignments_application_roles_appli~",
                        column: x => x.application_role_id,
                        principalSchema: "user_management",
                        principalTable: "application_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invite_role_application_assignments_invite_role_mappings_in~",
                        column: x => x.invite_role_mapping_id,
                        principalSchema: "user_management",
                        principalTable: "invite_role_mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invite_role_application_assignments_organisation_applicatio~",
                        column: x => x.organisation_application_id,
                        principalSchema: "user_management",
                        principalTable: "organisation_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_application_assignments",
                schema: "user_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_organisation_membership_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_application_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    assigned_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_application_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_application_assignments_organisation_applications_orga~",
                        column: x => x.organisation_application_id,
                        principalSchema: "user_management",
                        principalTable: "organisation_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_application_assignments_user_organisation_memberships_~",
                        column: x => x.user_organisation_membership_id,
                        principalSchema: "user_management",
                        principalTable: "user_organisation_memberships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_assignment_roles",
                schema: "user_management",
                columns: table => new
                {
                    user_assignment_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_assignment_roles", x => new { x.user_assignment_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_assignment_roles_application_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "user_management",
                        principalTable: "application_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_assignment_roles_user_application_assignments_user_ass~",
                        column: x => x.user_assignment_id,
                        principalSchema: "user_management",
                        principalTable: "user_application_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_application_permissions_app_name",
                schema: "user_management",
                table: "application_permissions",
                columns: new[] { "application_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_role_permissions_permission_id",
                schema: "user_management",
                table: "application_role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_roles_app_name",
                schema: "user_management",
                table: "application_roles",
                columns: new[] { "application_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_applications_client_id",
                schema: "user_management",
                table: "applications",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invite_role_app_assignments_mapping_app_role",
                schema: "user_management",
                table: "invite_role_application_assignments",
                columns: new[] { "invite_role_mapping_id", "organisation_application_id", "application_role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invite_role_application_assignments_application_role_id",
                schema: "user_management",
                table: "invite_role_application_assignments",
                column: "application_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_invite_role_application_assignments_organisation_applicatio~",
                schema: "user_management",
                table: "invite_role_application_assignments",
                column: "organisation_application_id");

            migrationBuilder.CreateIndex(
                name: "ix_invite_role_mappings_cdp_person_invite_guid",
                schema: "user_management",
                table: "invite_role_mappings",
                column: "cdp_person_invite_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invite_role_mappings_organisation_id",
                schema: "user_management",
                table: "invite_role_mappings",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "IX_organisation_applications_application_id",
                schema: "user_management",
                table: "organisation_applications",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_applications_org_app",
                schema: "user_management",
                table: "organisation_applications",
                columns: new[] { "organisation_id", "application_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisations_cdp_guid",
                schema: "user_management",
                table: "organisations",
                column: "cdp_organisation_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisations_slug",
                schema: "user_management",
                table: "organisations",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_app_assignments_membership_app",
                schema: "user_management",
                table: "user_application_assignments",
                columns: new[] { "user_organisation_membership_id", "organisation_application_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_application_assignments_organisation_application_id",
                schema: "user_management",
                table: "user_application_assignments",
                column: "organisation_application_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_assignment_roles_role_id",
                schema: "user_management",
                table: "user_assignment_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_org_memberships_cdp_person_id",
                schema: "user_management",
                table: "user_organisation_memberships",
                column: "cdp_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_org_memberships_user_org",
                schema: "user_management",
                table: "user_organisation_memberships",
                columns: new[] { "user_principal_id", "organisation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_organisation_memberships_organisation_id",
                schema: "user_management",
                table: "user_organisation_memberships",
                column: "organisation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_role_permissions",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "invite_role_application_assignments",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "user_assignment_roles",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "application_permissions",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "invite_role_mappings",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "application_roles",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "user_application_assignments",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "organisation_applications",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "user_organisation_memberships",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "applications",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "organisations",
                schema: "user_management");
        }
    }
}
