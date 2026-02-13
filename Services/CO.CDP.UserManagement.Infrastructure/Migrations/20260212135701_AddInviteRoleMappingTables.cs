using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteRoleMappingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invite_role_mappings",
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
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invite_role_application_assignments",
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
                        principalTable: "application_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invite_role_application_assignments_invite_role_mappings_in~",
                        column: x => x.invite_role_mapping_id,
                        principalTable: "invite_role_mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invite_role_application_assignments_organisation_applicatio~",
                        column: x => x.organisation_application_id,
                        principalTable: "organisation_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_invite_role_app_assignments_mapping_app_role",
                table: "invite_role_application_assignments",
                columns: new[] { "invite_role_mapping_id", "organisation_application_id", "application_role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invite_role_application_assignments_application_role_id",
                table: "invite_role_application_assignments",
                column: "application_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_invite_role_application_assignments_organisation_applicatio~",
                table: "invite_role_application_assignments",
                column: "organisation_application_id");

            migrationBuilder.CreateIndex(
                name: "ix_invite_role_mappings_cdp_person_invite_guid",
                table: "invite_role_mappings",
                column: "cdp_person_invite_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invite_role_mappings_organisation_id",
                table: "invite_role_mappings",
                column: "organisation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invite_role_application_assignments");

            migrationBuilder.DropTable(
                name: "invite_role_mappings");
        }
    }
}
