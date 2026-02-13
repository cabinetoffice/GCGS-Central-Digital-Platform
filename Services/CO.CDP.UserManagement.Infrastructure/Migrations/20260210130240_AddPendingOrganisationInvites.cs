using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingOrganisationInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pending_organisation_invites",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_role = table.Column<string>(type: "text", nullable: false),
                    cdp_person_invite_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pending_organisation_invites", x => x.id);
                    table.ForeignKey(
                        name: "FK_pending_organisation_invites_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pending_org_invites_cdp_person_invite_guid",
                table: "pending_organisation_invites",
                column: "cdp_person_invite_guid");

            migrationBuilder.CreateIndex(
                name: "ix_pending_org_invites_email_org",
                table: "pending_organisation_invites",
                columns: new[] { "email", "organisation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pending_organisation_invites_organisation_id",
                table: "pending_organisation_invites",
                column: "organisation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pending_organisation_invites");
        }
    }
}
