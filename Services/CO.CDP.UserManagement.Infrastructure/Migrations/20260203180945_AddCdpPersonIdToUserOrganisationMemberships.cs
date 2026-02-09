using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCdpPersonIdToUserOrganisationMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "cdp_person_id",
                table: "user_organisation_memberships",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_org_memberships_cdp_person_id",
                table: "user_organisation_memberships",
                column: "cdp_person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_org_memberships_cdp_person_id",
                table: "user_organisation_memberships");

            migrationBuilder.DropColumn(
                name: "cdp_person_id",
                table: "user_organisation_memberships");
        }
    }
}
